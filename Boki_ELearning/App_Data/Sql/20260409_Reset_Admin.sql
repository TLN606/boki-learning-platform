USE BokiDB;
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    DECLARE @AdminHash NVARCHAR(255) = N'PBKDF2$100000$zs4XhXDq78u6NnGvU1pghg==$AEUWKPyX46RoP0kFnkqWj3eSJg4IB/ssBBE5I5tGqiU=';

    IF EXISTS (SELECT 1 FROM dbo.TAIKHOAN WHERE EMAIL = N'admin@boki.edu.vn')
    BEGIN
        UPDATE dbo.TAIKHOAN
        SET TENDANGNHAP = N'admin',
            MATKHAU = @AdminHash,
            VAITRO = N'Admin'
        WHERE EMAIL = N'admin@boki.edu.vn';
    END
    ELSE
    BEGIN
        INSERT INTO dbo.TAIKHOAN (EMAIL, TENDANGNHAP, MATKHAU, VAITRO)
        VALUES (N'admin@boki.edu.vn', N'admin', @AdminHash, N'Admin');
    END;

    COMMIT;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK;

    THROW;
END CATCH;
