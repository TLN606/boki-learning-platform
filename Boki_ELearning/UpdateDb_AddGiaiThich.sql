-- =============================================
-- Script: Thêm cột GIAITHICH vào bảng CAUHOI
-- Mục đích: Lưu giải thích đáp án cho mỗi câu hỏi
-- Hướng dẫn: Chạy script này trong SQL Server Management Studio
-- =============================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'dbo.CAUHOI') 
    AND name = N'GIAITHICH'
)
BEGIN
    ALTER TABLE dbo.CAUHOI ADD GIAITHICH nvarchar(max) NULL;
    PRINT N'Đã thêm cột GIAITHICH vào bảng CAUHOI thành công.';
END
ELSE
BEGIN
    PRINT N'Cột GIAITHICH đã tồn tại trong bảng CAUHOI.';
END
GO
