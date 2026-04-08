[CmdletBinding()]
param(
    [ValidateSet("bak", "bacpac")]
    [string]$Format = "bak",

    [string]$ServerInstance,
    [string]$DatabaseName,
    [string]$Username,
    [string]$Password,
    [string]$OutputDir,
    [bool]$TrustServerCertificate = $true,

    [switch]$UseIntegratedSecurity
)

$ErrorActionPreference = "Stop"

function Get-ConnectionDefaultsFromWebConfig {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path)) {
        return @{}
    }

    try {
        [xml]$xml = Get-Content -LiteralPath $Path
        $connectionNode = $xml.SelectSingleNode("/configuration/connectionStrings/add")
        if (-not $connectionNode) {
            return @{}
        }

        $rawEntityConnection = [System.Net.WebUtility]::HtmlDecode([string]$connectionNode.GetAttribute("connectionString"))
        $providerConnection = $rawEntityConnection
        $providerMatch = [regex]::Match($rawEntityConnection, '(?i)provider connection string\s*=\s*"([^"]*)"')
        if ($providerMatch.Success) {
            $providerConnection = $providerMatch.Groups[1].Value
        }

        $providerBuilder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder $providerConnection

        $result = @{}
        $result["Data Source"] = [string]$providerBuilder["Data Source"]
        $result["Initial Catalog"] = [string]$providerBuilder["Initial Catalog"]
        $result["User ID"] = [string]$providerBuilder["User ID"]
        $result["Password"] = [string]$providerBuilder["Password"]
        $result["Integrated Security"] = [string]$providerBuilder["Integrated Security"]

        return $result
    }
    catch {
        Write-Verbose "Could not parse defaults from Web.config: $($_.Exception.Message)"
        return @{}
    }
}

function Resolve-AuthMode {
    param(
        [hashtable]$Defaults
    )

    if ($UseIntegratedSecurity.IsPresent) {
        return "Integrated"
    }

    $integratedFromConfig =
        ($Defaults.ContainsKey("Integrated Security") -and $Defaults["Integrated Security"].ToLower() -in @("true", "sspi")) -or
        ($Defaults.ContainsKey("Trusted_Connection") -and $Defaults["Trusted_Connection"].ToLower() -eq "true")

    if ($integratedFromConfig) {
        return "Integrated"
    }

    return "SqlAuth"
}

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = Resolve-Path (Join-Path $scriptDir "..")
$webConfigPath = Join-Path $projectRoot "Web.config"
$defaults = Get-ConnectionDefaultsFromWebConfig -Path $webConfigPath

if ([string]::IsNullOrWhiteSpace($ServerInstance) -and $defaults.ContainsKey("Data Source")) {
    $ServerInstance = $defaults["Data Source"]
}
if ([string]::IsNullOrWhiteSpace($ServerInstance)) {
    $ServerInstance = "."
}

if ([string]::IsNullOrWhiteSpace($DatabaseName) -and $defaults.ContainsKey("Initial Catalog")) {
    $DatabaseName = $defaults["Initial Catalog"]
}
if ([string]::IsNullOrWhiteSpace($DatabaseName)) {
    $DatabaseName = "BokiDB"
}

$authMode = Resolve-AuthMode -Defaults $defaults

if ($authMode -eq "SqlAuth") {
    if ([string]::IsNullOrWhiteSpace($Username) -and $defaults.ContainsKey("User ID")) {
        $Username = $defaults["User ID"]
    }
    if ([string]::IsNullOrWhiteSpace($Password) -and $defaults.ContainsKey("Password")) {
        $Password = $defaults["Password"]
    }

    if ([string]::IsNullOrWhiteSpace($Username) -or [string]::IsNullOrWhiteSpace($Password)) {
        throw "SQL Authentication requires -Username and -Password (or these values in Web.config)."
    }
}

if ([string]::IsNullOrWhiteSpace($OutputDir)) {
    $OutputDir = Join-Path $projectRoot "App_Data\Sql\Backups"
}

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$fileName = "{0}_{1}.{2}" -f $DatabaseName, $timestamp, $Format
$outputFile = Join-Path $OutputDir $fileName

if ($Format -eq "bak") {
    $sqlcmd = Get-Command SQLCMD.EXE -ErrorAction Stop
    $authArgs = @()
    if ($authMode -eq "Integrated") {
        $authArgs += "-E"
    }
    else {
        $authArgs += @("-U", $Username, "-P", $Password)
    }

    $outputFileSql = $outputFile.Replace("'", "''")
    $query = @"
BACKUP DATABASE [$DatabaseName]
TO DISK = N'$outputFileSql'
WITH COPY_ONLY, INIT, CHECKSUM, STATS = 10;
RESTORE VERIFYONLY FROM DISK = N'$outputFileSql';
"@

    $args = @("-S", $ServerInstance, "-b", "-Q", $query) + $authArgs
    if ($TrustServerCertificate) {
        $args += "-C"
    }
    & $sqlcmd.Source @args
    if ($LASTEXITCODE -ne 0) {
        throw "Backup failed (sqlcmd exit code $LASTEXITCODE)."
    }
}
else {
    $sqlPackage = Get-Command SqlPackage.exe -ErrorAction SilentlyContinue
    if (-not $sqlPackage) {
        $sqlPackage = Get-Command SqlPackage -ErrorAction SilentlyContinue
    }
    if (-not $sqlPackage) {
        throw "Format 'bacpac' requires SqlPackage in PATH. Install SQL Server Data-Tier Application Framework (DacFx) first."
    }

    if ($authMode -eq "Integrated") {
        $sourceConnectionString = "Server=$ServerInstance;Database=$DatabaseName;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True"
    }
    else {
        $sourceConnectionString = "Server=$ServerInstance;Database=$DatabaseName;User ID=$Username;Password=$Password;Encrypt=False;TrustServerCertificate=True"
    }

    $args = @(
        "/Action:Export",
        "/SourceConnectionString:$sourceConnectionString",
        "/TargetFile:$outputFile",
        "/p:VerifyExtraction=True",
        "/p:CommandTimeout=0"
    )

    & $sqlPackage.Source @args
    if ($LASTEXITCODE -ne 0) {
        throw "Export failed (SqlPackage exit code $LASTEXITCODE)."
    }
}

Write-Output ""
Write-Output "Backup complete:"
Write-Output "  File: $outputFile"
Write-Output "  Type: $Format"
Write-Output "  Server: $ServerInstance"
Write-Output "  Database: $DatabaseName"
