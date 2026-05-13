IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.CAUHOI')
      AND name = N'GIAITHICH'
)
BEGIN
    ALTER TABLE dbo.CAUHOI ADD GIAITHICH nvarchar(max) NULL;
END
GO
