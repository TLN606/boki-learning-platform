# BokiDB backup quick guide

This project includes a backup script for SQL Server database `BokiDB`.

Script path:
- `Boki_ELearning/Scripts/backup-bokidb.ps1`

## 1) Run backup to `.bak` (default)

From repository root:

```powershell
powershell -ExecutionPolicy Bypass -File .\Boki_ELearning\Scripts\backup-bokidb.ps1
```

The script reads defaults from `Boki_ELearning/Web.config` when possible.

Output default folder:
- `Boki_ELearning/App_Data/Sql/Backups`

## 2) Run backup with explicit credentials

```powershell
powershell -ExecutionPolicy Bypass -File .\Boki_ELearning\Scripts\backup-bokidb.ps1 `
  -ServerInstance "." `
  -DatabaseName "BokiDB" `
  -Username "sa" `
  -Password "your_password"
```

## 3) Export `.bacpac` (optional)

```powershell
powershell -ExecutionPolicy Bypass -File .\Boki_ELearning\Scripts\backup-bokidb.ps1 -Format bacpac
```

Notes:
- `bacpac` requires `SqlPackage` installed and available in PATH.
- If your backup file is over GitHub file size limit (100 MB), use Git LFS or external storage.
