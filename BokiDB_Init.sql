USE master;
GO

IF DB_ID('BokiDB') IS NOT NULL
BEGIN
    ALTER DATABASE BokiDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE BokiDB;
END
GO

CREATE DATABASE BokiDB;
GO

USE BokiDB;
GO

-- =======================================================
-- 1. TẠO CÁC BẢNG (TABLES)
-- =======================================================

CREATE TABLE LOP (
    MALOP INT IDENTITY(1,1) PRIMARY KEY,
    TENLOP NVARCHAR(50) NOT NULL
);

CREATE TABLE MONHOC (
    MAMONHOC INT IDENTITY(1,1) PRIMARY KEY,
    TENMONHOC NVARCHAR(100) NOT NULL
);

CREATE TABLE TAIKHOAN (
    MATAIKHOAN INT IDENTITY(1,1) PRIMARY KEY,
    EMAIL NVARCHAR(255) NOT NULL UNIQUE,
    TENDANGNHAP NVARCHAR(100) NOT NULL UNIQUE,
    MATKHAU NVARCHAR(255) NOT NULL, -- Dài 255 char để tương thích ASP.NET Identity Hash
    VAITRO NVARCHAR(50) NOT NULL CHECK (VAITRO IN ('HocSinh', 'GiaoVien', 'Admin'))
);

CREATE TABLE HOCSINH (
    MAHOCSINH INT IDENTITY(1,1) PRIMARY KEY,
    HOTEN NVARCHAR(255) NOT NULL,
    NGAYSINH DATE,
    GIOITINH NVARCHAR(10),
    NOIDUNGLYTUYET NVARCHAR(MAX),
    NGAYLAM DATE,
    MALOP INT NOT NULL,
    MATAIKHOAN INT NOT NULL UNIQUE,
    CONSTRAINT FK_HOCSINH_LOP FOREIGN KEY (MALOP) REFERENCES LOP(MALOP),
    CONSTRAINT FK_HOCSINH_TAIKHOAN FOREIGN KEY (MATAIKHOAN) REFERENCES TAIKHOAN(MATAIKHOAN)
);

CREATE TABLE BAIHOC (
    MABAIHOC INT IDENTITY(1,1) PRIMARY KEY,
    TENBAIHOC NVARCHAR(255) NOT NULL,
    NOIDUNGLYTUYET NVARCHAR(MAX),
    MEOGHINHO NVARCHAR(MAX),
    MAMONHOC INT NOT NULL,
    MALOP INT NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0, -- Soft Delete Strategy
    CONSTRAINT FK_BAIHOC_MONHOC FOREIGN KEY (MAMONHOC) REFERENCES MONHOC(MAMONHOC),
    CONSTRAINT FK_BAIHOC_LOP FOREIGN KEY (MALOP) REFERENCES LOP(MALOP)
);

CREATE TABLE CAUHOI (
    MACAUHOI INT IDENTITY(1,1) PRIMARY KEY,
    NOIDUNGCAUHOI NVARCHAR(MAX) NOT NULL,
    MABAIHOC INT NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0, -- Soft Delete Strategy
    CONSTRAINT FK_CAUHOI_BAIHOC FOREIGN KEY (MABAIHOC) REFERENCES BAIHOC(MABAIHOC)
);

CREATE TABLE DAPAN (
    MADAPAN INT IDENTITY(1,1) PRIMARY KEY,
    NOIDUNGDAPAN NVARCHAR(MAX) NOT NULL,
    DUNGLSAI BIT NOT NULL,
    MAMONHOC INT, 
    MACAUHOI INT NOT NULL,
    IsDeleted BIT NOT NULL DEFAULT 0, -- Soft Delete Strategy
    CONSTRAINT FK_DAPAN_CAUHOI FOREIGN KEY (MACAUHOI) REFERENCES CAUHOI(MACAUHOI)
);

CREATE TABLE KETQUA (
    MAKETQUA INT IDENTITY(1,1) PRIMARY KEY,
    DIEMSO FLOAT NOT NULL,
    NOIDUNGLYTUYET NVARCHAR(MAX),
    NGAYLAM DATE NOT NULL DEFAULT GETDATE(),
    MAHOCSINH INT NOT NULL,
    MABAIHOC INT NOT NULL,
    CONSTRAINT FK_KETQUA_HOCSINH FOREIGN KEY (MAHOCSINH) REFERENCES HOCSINH(MAHOCSINH),
    CONSTRAINT FK_KETQUA_BAIHOC FOREIGN KEY (MABAIHOC) REFERENCES BAIHOC(MABAIHOC)
);

CREATE TABLE CHITIKETQUA (
    MACHITIKET INT IDENTITY(1,1) PRIMARY KEY,
    MAKETQUA INT NOT NULL,
    MACAUHOI INT NOT NULL,
    MADAPANCHON INT NOT NULL,
    DUNG BIT NOT NULL,
    CONSTRAINT FK_CHITIKETQUA_KETQUA FOREIGN KEY (MAKETQUA) REFERENCES KETQUA(MAKETQUA),
    CONSTRAINT FK_CHITIKETQUA_CAUHOI FOREIGN KEY (MACAUHOI) REFERENCES CAUHOI(MACAUHOI),
    CONSTRAINT FK_CHITIKETQUA_DAPAN FOREIGN KEY (MADAPANCHON) REFERENCES DAPAN(MADAPAN)
);
GO

-- =======================================================
-- 2. TẠO INDEX CHO FOREIGN KEYS (TỐI ƯU TRUY VẤN)
-- =======================================================
CREATE INDEX IX_HOCSINH_MALOP ON HOCSINH(MALOP);
CREATE INDEX IX_BAIHOC_MAMONHOC ON BAIHOC(MAMONHOC);
CREATE INDEX IX_BAIHOC_MALOP ON BAIHOC(MALOP);
CREATE INDEX IX_CAUHOI_MABAIHOC ON CAUHOI(MABAIHOC);
CREATE INDEX IX_DAPAN_MACAUHOI ON DAPAN(MACAUHOI);
CREATE INDEX IX_KETQUA_MAHOCSINH ON KETQUA(MAHOCSINH);
CREATE INDEX IX_KETQUA_MABAIHOC ON KETQUA(MABAIHOC);
CREATE INDEX IX_CHITIKETQUA_MAKETQUA ON CHITIKETQUA(MAKETQUA);
GO

-- =======================================================
-- 3. STORED PROCEDURES KHAI THÁC DỮ LIỆU CHÍNH
-- =======================================================

-- 3.1 sp_GetBaiHocByLopAndMon (Chỉ lấy các bài chưa bị Soft Delete)
CREATE OR ALTER PROCEDURE sp_GetBaiHocByLopAndMon
    @MaLop INT,
    @MaMonHoc INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT * 
    FROM BAIHOC 
    WHERE MALOP = @MaLop 
      AND MAMONHOC = @MaMonHoc 
      AND IsDeleted = 0;
END
GO

-- 3.2 sp_GetCauHoiByBaiHoc (Bao gồm Câu hỏi + Đáp án lồng nhau, kiểm tra Soft Delete)
CREATE OR ALTER PROCEDURE sp_GetCauHoiByBaiHoc
    @MaBaiHoc INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT 
        C.MACAUHOI, C.NOIDUNGCAUHOI, 
        D.MADAPAN, D.NOIDUNGDAPAN, D.DUNGLSAI
    FROM CAUHOI C
    INNER JOIN DAPAN D ON C.MACAUHOI = D.MACAUHOI
    WHERE C.MABAIHOC = @MaBaiHoc 
      AND C.IsDeleted = 0 
      AND D.IsDeleted = 0;
END
GO

-- 3.3 sp_LuuKetQua (Lưu tổng điểm làm bài mới)
CREATE OR ALTER PROCEDURE sp_LuuKetQua
    @MaHocSinh INT,
    @MaBaiHoc INT,
    @DiemSo FLOAT,
    @NoiDungLyTuyet NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    INSERT INTO KETQUA (DIEMSO, NOIDUNGLYTUYET, NGAYLAM, MAHOCSINH, MABAIHOC)
    VALUES (@DiemSo, @NoiDungLyTuyet, GETDATE(), @MaHocSinh, @MaBaiHoc);
    
    -- Trả ra ID vừa insert để thêm chi tiết ở tầng C# nếu cần
    SELECT SCOPE_IDENTITY() AS NewKetQuaId;
END
GO

-- 3.4 sp_GetKetQuaChiTiet (Xem lại cụ thể học sinh chọn đáp án nào, đúng/sai)
CREATE OR ALTER PROCEDURE sp_GetKetQuaChiTiet
    @MaKetQua INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        C.NOIDUNGCAUHOI,
        D.NOIDUNGDAPAN AS DapAnHocSinhChon,
        CK.DUNG AS TrangThaiTraLoi
    FROM CHITIKETQUA CK
    INNER JOIN CAUHOI C ON CK.MACAUHOI = C.MACAUHOI
    INNER JOIN DAPAN D ON CK.MADAPANCHON = D.MADAPAN
    WHERE CK.MAKETQUA = @MaKetQua;
END
GO

-- 3.5 sp_GetTienDoHocSinh (Summary điểm các bài đã test)
CREATE OR ALTER PROCEDURE sp_GetTienDoHocSinh
    @MaHocSinh INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        L.TENLOP, M.TENMONHOC, B.TENBAIHOC,
        K.DIEMSO, K.NGAYLAM
    FROM KETQUA K
    INNER JOIN BAIHOC B ON K.MABAIHOC = B.MABAIHOC
    INNER JOIN LOP L ON B.MALOP = L.MALOP
    INNER JOIN MONHOC M ON B.MAMONHOC = M.MAMONHOC
    WHERE K.MAHOCSINH = @MaHocSinh
    ORDER BY K.NGAYLAM DESC;
END
GO


-- =======================================================
-- 4. INSERT DỮ LIỆU MẪU (MOCK DATA)
-- =======================================================
SET NOCOUNT ON;

-- 4.1 Thêm 4 Lớp Học (Lớp 2-5)
INSERT INTO LOP (TENLOP) VALUES (N'Lớp 2'), (N'Lớp 3'), (N'Lớp 4'), (N'Lớp 5');

-- 4.2 Thêm 3 Môn Học
INSERT INTO MONHOC (TENMONHOC) VALUES (N'Toán'), (N'Tiếng Việt'), (N'Tiếng Anh');

-- 4.3 Thêm 1 Tài khoản Admin
INSERT INTO TAIKHOAN (EMAIL, TENDANGNHAP, MATKHAU, VAITRO) 
VALUES ('admin@boki.edu.vn', 'admin', 'AQAAAAEAACcQAAAA...fakehash', 'Admin');

-- 4.4 Thêm 5 Học Sinh & Tài khoản tương ứng
DECLARE @i INT = 1;
WHILE @i <= 5
BEGIN
    INSERT INTO TAIKHOAN (EMAIL, TENDANGNHAP, MATKHAU, VAITRO)
    VALUES ('hs' + CAST(@i AS VARCHAR) + '@boki.edu.vn', 'hocsinh' + CAST(@i AS VARCHAR), 'AQAAAAEAACcQAAAA...fakehash', 'HocSinh');
    
    DECLARE @MaTk INT = SCOPE_IDENTITY();
    
    -- Xoay vòng các lớp từ 1 đến 4
    DECLARE @MaLop INT = ((@i - 1) % 4) + 1;
    
    INSERT INTO HOCSINH (HOTEN, NGAYSINH, GIOITINH, MALOP, MATAIKHOAN)
    VALUES (N'Học sinh mẫu ' + CAST(@i AS NVARCHAR), DATEADD(YEAR, -5 - @MaLop, GETDATE()), N'Nam', @MaLop, @MaTk);
    
    SET @i = @i + 1;
END

-- 4.5 Cắm Dữ Liệu Bài Học, Câu Hỏi, Đáp Án
DECLARE @LopId INT = 1;
WHILE @LopId <= 4
BEGIN
    DECLARE @MonId INT = 1;
    WHILE @MonId <= 3
    BEGIN
        -- Sinh 2 Bài học mỗi môn/lớp
        DECLARE @BaiHocId INT = 1;
        WHILE @BaiHocId <= 2
        BEGIN
            INSERT INTO BAIHOC (TENBAIHOC, NOIDUNGLYTUYET, MEOGHINHO, MAMONHOC, MALOP)
            VALUES (
                N'Bài số ' + CAST(@BaiHocId AS NVARCHAR) + N': ' + 
                    (SELECT TENMONHOC FROM MONHOC WHERE MAMONHOC = @MonId) + 
                    N' ' + (SELECT TENLOP FROM LOP WHERE MALOP = @LopId),
                N'Đây là nội dung lý thuyết giả lập giúp học sinh hiểu bài...',
                N'Mẹo: Cứ thấy bài khó thì đừng nản, làm lại từ từ !',
                @MonId, @LopId
            );
            
            DECLARE @CurrentBaiHoc INT = SCOPE_IDENTITY();
            
            -- Sinh 5 Câu hỏi cho bài học này
            DECLARE @CauHoiId INT = 1;
            WHILE @CauHoiId <= 5
            BEGIN
                INSERT INTO CAUHOI (NOIDUNGCAUHOI, MABAIHOC)
                VALUES (N'Nội dung câu hỏi rèn luyện số ' + CAST(@CauHoiId AS NVARCHAR) + N' là gì?', @CurrentBaiHoc);
                
                DECLARE @CurrentCauHoi INT = SCOPE_IDENTITY();
                
                -- Tạo ra 4 đáp án (A,B,C,D)
                DECLARE @DapAnId INT = 1;
                WHILE @DapAnId <= 4
                BEGIN
                    DECLARE @IsDung BIT = 0;
                    IF @DapAnId = 1 SET @IsDung = 1; -- Mã định đáp án 1 luôn đúng
                    
                    INSERT INTO DAPAN (NOIDUNGDAPAN, DUNGLSAI, MAMONHOC, MACAUHOI)
                    VALUES (N'Đáp án ' + CHAR(64 + @DapAnId) + N' cho câu hỏi ' + CAST(@CauHoiId AS NVARCHAR), 
                            @IsDung, @MonId, @CurrentCauHoi);
                    
                    SET @DapAnId = @DapAnId + 1;
                END
                
                SET @CauHoiId = @CauHoiId + 1;
            END
            
            SET @BaiHocId = @BaiHocId + 1;
        END
        SET @MonId = @MonId + 1;
    END
    SET @LopId = @LopId + 1;
END
GO

PRINT 'BOKI DATABASE SETUP COMPLETED SUCCESSFULLY!'
