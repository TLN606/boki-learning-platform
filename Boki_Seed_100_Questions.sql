SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    DECLARE @PreActiveCauHoi INT = (SELECT COUNT(1) FROM CAUHOI WHERE IsDeleted = 0);
    DECLARE @PreActiveDapAn INT = (SELECT COUNT(1) FROM DAPAN WHERE IsDeleted = 0);
    DECLARE @PreActiveNonPlaceholderCauHoi INT = (
        SELECT COUNT(1)
        FROM CAUHOI
        WHERE IsDeleted = 0
          AND NOIDUNGCAUHOI NOT LIKE N'Nội dung câu hỏi rèn luyện số % là gì?'
    );
    DECLARE @PreActiveNonPlaceholderDapAn INT = (
        SELECT COUNT(1)
        FROM DAPAN D
        INNER JOIN CAUHOI C ON C.MACAUHOI = D.MACAUHOI
        WHERE D.IsDeleted = 0
          AND C.IsDeleted = 0
          AND C.NOIDUNGCAUHOI NOT LIKE N'Nội dung câu hỏi rèn luyện số % là gì?'
          AND D.NOIDUNGDAPAN NOT LIKE N'Đáp án % cho câu hỏi %'
    );

    IF OBJECT_ID('tempdb..#ExpectedPairs') IS NOT NULL DROP TABLE #ExpectedPairs;
    CREATE TABLE #ExpectedPairs (
        TENLOP NVARCHAR(50) NOT NULL,
        TENMONHOC NVARCHAR(50) NOT NULL,
        PRIMARY KEY (TENLOP, TENMONHOC)
    );

    INSERT INTO #ExpectedPairs (TENLOP, TENMONHOC)
    VALUES
        (N'Lớp 2', N'Toán'), (N'Lớp 2', N'Tiếng Việt'), (N'Lớp 2', N'Tiếng Anh'),
        (N'Lớp 3', N'Toán'), (N'Lớp 3', N'Tiếng Việt'), (N'Lớp 3', N'Tiếng Anh'),
        (N'Lớp 4', N'Toán'), (N'Lớp 4', N'Tiếng Việt'), (N'Lớp 4', N'Tiếng Anh'),
        (N'Lớp 5', N'Toán'), (N'Lớp 5', N'Tiếng Việt'), (N'Lớp 5', N'Tiếng Anh');

    IF OBJECT_ID('tempdb..#TargetLessons') IS NOT NULL DROP TABLE #TargetLessons;
    CREATE TABLE #TargetLessons (
        TENLOP NVARCHAR(50) NOT NULL,
        TENMONHOC NVARCHAR(50) NOT NULL,
        MABAIHOC INT NOT NULL,
        MAMONHOC INT NOT NULL,
        PRIMARY KEY (TENLOP, TENMONHOC)
    );

    INSERT INTO #TargetLessons (TENLOP, TENMONHOC, MABAIHOC, MAMONHOC)
    SELECT
        L.TENLOP,
        M.TENMONHOC,
        MIN(B.MABAIHOC) AS MABAIHOC,
        M.MAMONHOC
    FROM BAIHOC B
    INNER JOIN LOP L ON L.MALOP = B.MALOP
    INNER JOIN MONHOC M ON M.MAMONHOC = B.MAMONHOC
    WHERE B.IsDeleted = 0
      AND L.TENLOP IN (N'Lớp 2', N'Lớp 3', N'Lớp 4', N'Lớp 5')
      AND M.TENMONHOC IN (N'Toán', N'Tiếng Việt', N'Tiếng Anh')
    GROUP BY L.TENLOP, M.TENMONHOC, M.MAMONHOC;

    IF EXISTS (
        SELECT 1
        FROM #ExpectedPairs E
        LEFT JOIN #TargetLessons T
            ON T.TENLOP = E.TENLOP
           AND T.TENMONHOC = E.TENMONHOC
        WHERE T.MABAIHOC IS NULL
    )
    BEGIN
        THROW 51001, N'Thieu bai hoc active cho it nhat 1 cap (Lop, Mon) trong pham vi seed.', 1;
    END;

    UPDATE D
    SET D.IsDeleted = 1
    FROM DAPAN D
    INNER JOIN CAUHOI C ON C.MACAUHOI = D.MACAUHOI
    WHERE D.IsDeleted = 0
      AND (
            D.NOIDUNGDAPAN LIKE N'Đáp án % cho câu hỏi %'
            OR C.NOIDUNGCAUHOI LIKE N'Nội dung câu hỏi rèn luyện số % là gì?'
          );

    UPDATE C
    SET C.IsDeleted = 1
    FROM CAUHOI C
    WHERE C.IsDeleted = 0
      AND C.NOIDUNGCAUHOI LIKE N'Nội dung câu hỏi rèn luyện số % là gì?';

    IF OBJECT_ID('tempdb..#SeedQuestions') IS NOT NULL DROP TABLE #SeedQuestions;
    CREATE TABLE #SeedQuestions (
        SeedId INT IDENTITY(1,1) PRIMARY KEY,
        TENLOP NVARCHAR(255) NOT NULL,
        TENMONHOC NVARCHAR(255) NOT NULL,
        QuestionText NVARCHAR(MAX) NOT NULL,
        OptionA NVARCHAR(MAX) NOT NULL,
        OptionB NVARCHAR(MAX) NOT NULL,
        OptionC NVARCHAR(MAX) NOT NULL,
        OptionD NVARCHAR(MAX) NOT NULL,
        CorrectOption CHAR(1) NOT NULL
    );

    INSERT INTO #SeedQuestions (TENLOP, TENMONHOC, QuestionText, OptionA, OptionB, OptionC, OptionD, CorrectOption)
    VALUES
(N'Lớp 2', N'Toán', N'[Lớp 2 - Toán] Tính: 24 + 13 = ?', N'37', N'35', N'40', N'43', 'A'),
(N'Lớp 2', N'Toán', N'[Lớp 2 - Toán] Tính: 70 - 16 = ?', N'53', N'54', N'56', N'59', 'B'),
(N'Lớp 2', N'Toán', N'[Lớp 2 - Toán] Kết quả của 9 x 8 là bao nhiêu?', N'63', N'80', N'72', N'76', 'C'),
(N'Lớp 2', N'Toán', N'[Lớp 2 - Toán] Tính: 40 : 8 = ?', N'4', N'7', N'9', N'5', 'D'),
(N'Lớp 2', N'Toán', N'[Lớp 2 - Toán] Hình chữ nhật có chiều dài 8 cm và chiều rộng 6 cm. Chu vi là bao nhiêu?', N'28 cm', N'14 cm', N'32 cm', N'26 cm', 'A'),
(N'Lớp 2', N'Toán', N'[Lớp 2 - Toán] Hình chữ nhật dài 9 cm, rộng 7 cm. Diện tích là:', N'16 cm2', N'63 cm2', N'70 cm2', N'54 cm2', 'B'),
(N'Lớp 2', N'Toán', N'[Lớp 2 - Toán] Số tiếp theo của dãy 11, 14, 17, ... là số nào?', N'19', N'22', N'20', N'23', 'C'),
(N'Lớp 2', N'Toán', N'[Lớp 2 - Toán] Mỗi túi có 5 viên bi. Có 6 túi và thêm 9 viên bi rời. Tất cả có bao nhiêu viên bi?', N'33', N'44', N'37', N'39', 'D'),
(N'Lớp 2', N'Toán', N'[Lớp 2 - Toán] Tính: 32 + 13 = ?', N'45', N'43', N'48', N'51', 'A'),
(N'Lớp 2', N'Tiếng Việt', N'[Lớp 2 - Tiếng Việt] Từ nào đồng nghĩa với ''chăm chỉ''?', N'sáng dạ', N'siêng năng', N'lanh lợi', N'yên tĩnh', 'B'),
(N'Lớp 2', N'Tiếng Việt', N'[Lớp 2 - Tiếng Việt] Từ nào trái nghĩa với ''sáng''?', N'lanh lợi', N'yên tĩnh', N'tối', N'khổng lồ', 'C'),
(N'Lớp 2', N'Tiếng Việt', N'[Lớp 2 - Tiếng Việt] Từ nào đồng nghĩa với ''vui vẻ''?', N'yên tĩnh', N'khổng lồ', N'đúng đắn', N'phấn khởi', 'D'),
(N'Lớp 2', N'Tiếng Việt', N'[Lớp 2 - Tiếng Việt] Từ nào trái nghĩa với ''mạnh''?', N'yếu', N'khổng lồ', N'đúng đắn', N'ủ rũ', 'A'),
(N'Lớp 2', N'Tiếng Việt', N'[Lớp 2 - Tiếng Việt] Từ nào đồng nghĩa với ''thông minh''?', N'đúng đắn', N'sáng dạ', N'ủ rũ', N'bền bỉ', 'B'),
(N'Lớp 2', N'Tiếng Việt', N'[Lớp 2 - Tiếng Việt] Từ nào trái nghĩa với ''gần''?', N'ủ rũ', N'bền bỉ', N'xa', N'ngăn nắp', 'C'),
(N'Lớp 2', N'Tiếng Việt', N'[Lớp 2 - Tiếng Việt] Từ nào đồng nghĩa với ''im lặng''?', N'bền bỉ', N'ngăn nắp', N'bé xíu', N'yên tĩnh', 'D'),
(N'Lớp 2', N'Tiếng Việt', N'[Lớp 2 - Tiếng Việt] Từ nào trái nghĩa với ''vui''?', N'buồn', N'ngăn nắp', N'bé xíu', N'thật thà', 'A'),
(N'Lớp 2', N'Tiếng Anh', N'[Lớp 2 - Tiếng Anh] Từ tiếng Anh nào có nghĩa là ''sách''?', N'cat', N'book', N'dog', N'sun', 'B'),
(N'Lớp 2', N'Tiếng Anh', N'[Lớp 2 - Tiếng Anh] Nghĩa tiếng Việt của từ ''school'' là gì?', N'con chó', N'mặt trời', N'trường học', N'mặt trăng', 'C'),
(N'Lớp 2', N'Tiếng Anh', N'[Lớp 2 - Tiếng Anh] Từ tiếng Anh nào có nghĩa là ''giáo viên''?', N'sun', N'moon', N'morning', N'teacher', 'D'),
(N'Lớp 2', N'Tiếng Anh', N'[Lớp 2 - Tiếng Anh] Nghĩa tiếng Việt của từ ''student'' là gì?', N'học sinh', N'mặt trăng', N'buổi sáng', N'buổi tối', 'A'),
(N'Lớp 2', N'Tiếng Anh', N'[Lớp 2 - Tiếng Anh] Từ tiếng Anh nào có nghĩa là ''quả táo''?', N'morning', N'apple', N'evening', N'happy', 'B'),
(N'Lớp 2', N'Tiếng Anh', N'[Lớp 2 - Tiếng Anh] Nghĩa tiếng Việt của từ ''water'' là gì?', N'buổi tối', N'vui vẻ', N'nước', N'buồn', 'C'),
(N'Lớp 2', N'Tiếng Anh', N'[Lớp 2 - Tiếng Anh] Từ tiếng Anh nào có nghĩa là ''con mèo''?', N'happy', N'sad', N'big', N'cat', 'D'),
(N'Lớp 2', N'Tiếng Anh', N'[Lớp 2 - Tiếng Anh] Nghĩa tiếng Việt của từ ''dog'' là gì?', N'con chó', N'buồn', N'to lớn', N'nhỏ', 'A'),
(N'Lớp 3', N'Toán', N'[Lớp 3 - Toán] Tính: 28 + 14 = ?', N'40', N'42', N'45', N'48', 'B'),
(N'Lớp 3', N'Toán', N'[Lớp 3 - Toán] Tính: 79 - 17 = ?', N'61', N'64', N'62', N'67', 'C'),
(N'Lớp 3', N'Toán', N'[Lớp 3 - Toán] Kết quả của 10 x 3 là bao nhiêu?', N'20', N'33', N'34', N'30', 'D'),
(N'Lớp 3', N'Toán', N'[Lớp 3 - Toán] Tính: 12 : 2 = ?', N'6', N'5', N'8', N'10', 'A'),
(N'Lớp 3', N'Toán', N'[Lớp 3 - Toán] Hình chữ nhật có chiều dài 9 cm và chiều rộng 3 cm. Chu vi là bao nhiêu?', N'12 cm', N'24 cm', N'28 cm', N'22 cm', 'B'),
(N'Lớp 3', N'Toán', N'[Lớp 3 - Toán] Hình chữ nhật dài 10 cm, rộng 8 cm. Diện tích là:', N'18 cm2', N'88 cm2', N'80 cm2', N'70 cm2', 'C'),
(N'Lớp 3', N'Toán', N'[Lớp 3 - Toán] Số tiếp theo của dãy 13, 17, 21, ... là số nào?', N'24', N'27', N'29', N'25', 'D'),
(N'Lớp 3', N'Toán', N'[Lớp 3 - Toán] Mỗi túi có 6 viên bi. Có 4 túi và thêm 10 viên bi rời. Tất cả có bao nhiêu viên bi?', N'34', N'30', N'40', N'32', 'A'),
(N'Lớp 3', N'Tiếng Việt', N'[Lớp 3 - Tiếng Việt] Từ nào đồng nghĩa với ''chính xác''?', N'bé xíu', N'đúng đắn', N'thật thà', N'gắn bó', 'B'),
(N'Lớp 3', N'Tiếng Việt', N'[Lớp 3 - Tiếng Việt] Từ nào trái nghĩa với ''dày''?', N'thật thà', N'gắn bó', N'mỏng', N'đẹp đẽ', 'C'),
(N'Lớp 3', N'Tiếng Việt', N'[Lớp 3 - Tiếng Việt] Từ nào đồng nghĩa với ''kiên trì''?', N'gắn bó', N'đẹp đẽ', N'kính trọng', N'bền bỉ', 'D'),
(N'Lớp 3', N'Tiếng Việt', N'[Lớp 3 - Tiếng Việt] Từ nào trái nghĩa với ''đầu''?', N'cuối', N'đẹp đẽ', N'kính trọng', N'thận trọng', 'A'),
(N'Lớp 3', N'Tiếng Việt', N'[Lớp 3 - Tiếng Việt] Từ nào đồng nghĩa với ''nhỏ bé''?', N'kính trọng', N'bé xíu', N'thận trọng', N'từ tốn', 'B'),
(N'Lớp 3', N'Tiếng Việt', N'[Lớp 3 - Tiếng Việt] Từ nào trái nghĩa với ''trong''?', N'thận trọng', N'từ tốn', N'ngoài', N'ấm cúng', 'C'),
(N'Lớp 3', N'Tiếng Việt', N'[Lớp 3 - Tiếng Việt] Từ nào đồng nghĩa với ''đoàn kết''?', N'từ tốn', N'ấm cúng', N'thấp', N'gắn bó', 'D'),
(N'Lớp 3', N'Tiếng Việt', N'[Lớp 3 - Tiếng Việt] Từ nào trái nghĩa với ''mới''?', N'cũ', N'ấm cúng', N'thấp', N'tối', 'A'),
(N'Lớp 3', N'Tiếng Việt', N'[Lớp 3 - Tiếng Việt] Từ nào đồng nghĩa với ''lễ phép''?', N'thấp', N'kính trọng', N'tối', N'chậm', 'B'),
(N'Lớp 3', N'Tiếng Anh', N'[Lớp 3 - Tiếng Anh] Từ tiếng Anh nào có nghĩa là ''mặt trời''?', N'big', N'small', N'sun', N'open', 'C'),
(N'Lớp 3', N'Tiếng Anh', N'[Lớp 3 - Tiếng Anh] Nghĩa tiếng Việt của từ ''moon'' là gì?', N'nhỏ', N'mở', N'đóng', N'mặt trăng', 'D'),
(N'Lớp 3', N'Tiếng Anh', N'[Lớp 3 - Tiếng Anh] Từ tiếng Anh nào có nghĩa là ''buổi sáng''?', N'morning', N'open', N'close', N'read', 'A'),
(N'Lớp 3', N'Tiếng Anh', N'[Lớp 3 - Tiếng Anh] Nghĩa tiếng Việt của từ ''evening'' là gì?', N'đóng', N'buổi tối', N'đọc', N'viết', 'B'),
(N'Lớp 3', N'Tiếng Anh', N'[Lớp 3 - Tiếng Anh] Từ tiếng Anh nào có nghĩa là ''vui vẻ''?', N'read', N'write', N'happy', N'listen', 'C'),
(N'Lớp 3', N'Tiếng Anh', N'[Lớp 3 - Tiếng Anh] Nghĩa tiếng Việt của từ ''sad'' là gì?', N'viết', N'nghe', N'nói', N'buồn', 'D'),
(N'Lớp 3', N'Tiếng Anh', N'[Lớp 3 - Tiếng Anh] Từ tiếng Anh nào có nghĩa là ''to lớn''?', N'big', N'listen', N'speak', N'red', 'A'),
(N'Lớp 3', N'Tiếng Anh', N'[Lớp 3 - Tiếng Anh] Nghĩa tiếng Việt của từ ''small'' là gì?', N'nói', N'nhỏ', N'màu đỏ', N'màu xanh dương', 'B'),
(N'Lớp 4', N'Toán', N'[Lớp 4 - Toán] Tính: 32 + 15 = ?', N'45', N'50', N'47', N'53', 'C'),
(N'Lớp 4', N'Toán', N'[Lớp 4 - Toán] Tính: 88 - 18 = ?', N'69', N'72', N'75', N'70', 'D'),
(N'Lớp 4', N'Toán', N'[Lớp 4 - Toán] Kết quả của 11 x 4 là bao nhiêu?', N'44', N'33', N'48', N'51', 'A'),
(N'Lớp 4', N'Toán', N'[Lớp 4 - Toán] Tính: 21 : 3 = ?', N'6', N'7', N'9', N'11', 'B'),
(N'Lớp 4', N'Toán', N'[Lớp 4 - Toán] Hình chữ nhật có chiều dài 10 cm và chiều rộng 4 cm. Chu vi là bao nhiêu?', N'14 cm', N'32 cm', N'28 cm', N'26 cm', 'C'),
(N'Lớp 4', N'Toán', N'[Lớp 4 - Toán] Hình chữ nhật dài 11 cm, rộng 4 cm. Diện tích là:', N'15 cm2', N'48 cm2', N'33 cm2', N'44 cm2', 'D'),
(N'Lớp 4', N'Toán', N'[Lớp 4 - Toán] Số tiếp theo của dãy 15, 20, 25, ... là số nào?', N'30', N'29', N'32', N'35', 'A'),
(N'Lớp 4', N'Toán', N'[Lớp 4 - Toán] Mỗi túi có 3 viên bi. Có 5 túi và thêm 11 viên bi rời. Tất cả có bao nhiêu viên bi?', N'21', N'26', N'29', N'24', 'B'),
(N'Lớp 4', N'Tiếng Việt', N'[Lớp 4 - Tiếng Việt] Từ nào trái nghĩa với ''rộng''?', N'tối', N'chậm', N'hẹp', N'yếu', 'C'),
(N'Lớp 4', N'Tiếng Việt', N'[Lớp 4 - Tiếng Việt] Từ nào đồng nghĩa với ''chậm rãi''?', N'chậm', N'yếu', N'ướt', N'từ tốn', 'D'),
(N'Lớp 4', N'Tiếng Việt', N'[Lớp 4 - Tiếng Việt] Từ nào trái nghĩa với ''đắt''?', N'rẻ', N'yếu', N'ướt', N'xa', 'A'),
(N'Lớp 4', N'Tiếng Việt', N'[Lớp 4 - Tiếng Việt] Từ nào đồng nghĩa với ''chăm chỉ''?', N'ướt', N'siêng năng', N'xa', N'đóng', 'B'),
(N'Lớp 4', N'Tiếng Việt', N'[Lớp 4 - Tiếng Việt] Từ nào trái nghĩa với ''sáng''?', N'xa', N'đóng', N'tối', N'buồn', 'C'),
(N'Lớp 4', N'Tiếng Việt', N'[Lớp 4 - Tiếng Việt] Từ nào đồng nghĩa với ''vui vẻ''?', N'đóng', N'buồn', N'vơi', N'phấn khởi', 'D'),
(N'Lớp 4', N'Tiếng Việt', N'[Lớp 4 - Tiếng Việt] Từ nào trái nghĩa với ''mạnh''?', N'yếu', N'buồn', N'vơi', N'mỏng', 'A'),
(N'Lớp 4', N'Tiếng Việt', N'[Lớp 4 - Tiếng Việt] Từ nào đồng nghĩa với ''thông minh''?', N'vơi', N'sáng dạ', N'mỏng', N'lạnh', 'B'),
(N'Lớp 4', N'Tiếng Anh', N'[Lớp 4 - Tiếng Anh] Từ tiếng Anh nào có nghĩa là ''mở''?', N'red', N'blue', N'open', N'green', 'C'),
(N'Lớp 4', N'Tiếng Anh', N'[Lớp 4 - Tiếng Anh] Nghĩa tiếng Việt của từ ''close'' là gì?', N'màu xanh dương', N'màu xanh lá', N'màu vàng', N'đóng', 'D'),
(N'Lớp 4', N'Tiếng Anh', N'[Lớp 4 - Tiếng Anh] Từ tiếng Anh nào có nghĩa là ''đọc''?', N'read', N'green', N'yellow', N'family', 'A'),
(N'Lớp 4', N'Tiếng Anh', N'[Lớp 4 - Tiếng Anh] Nghĩa tiếng Việt của từ ''write'' là gì?', N'màu vàng', N'viết', N'gia đình', N'bạn bè', 'B'),
(N'Lớp 4', N'Tiếng Anh', N'[Lớp 4 - Tiếng Anh] Từ tiếng Anh nào có nghĩa là ''nghe''?', N'family', N'friend', N'listen', N'house', 'C'),
(N'Lớp 4', N'Tiếng Anh', N'[Lớp 4 - Tiếng Anh] Nghĩa tiếng Việt của từ ''speak'' là gì?', N'bạn bè', N'ngôi nhà', N'lớp học', N'nói', 'D'),
(N'Lớp 4', N'Tiếng Anh', N'[Lớp 4 - Tiếng Anh] Từ tiếng Anh nào có nghĩa là ''màu đỏ''?', N'red', N'house', N'classroom', N'pencil', 'A'),
(N'Lớp 4', N'Tiếng Anh', N'[Lớp 4 - Tiếng Anh] Nghĩa tiếng Việt của từ ''blue'' là gì?', N'lớp học', N'màu xanh dương', N'bút chì', N'quyển vở', 'B'),
(N'Lớp 4', N'Tiếng Anh', N'[Lớp 4 - Tiếng Anh] Từ tiếng Anh nào có nghĩa là ''màu xanh lá''?', N'pencil', N'notebook', N'green', N'chair', 'C'),
(N'Lớp 5', N'Toán', N'[Lớp 5 - Toán] Tính: 36 + 16 = ?', N'50', N'55', N'58', N'52', 'D'),
(N'Lớp 5', N'Toán', N'[Lớp 5 - Toán] Tính: 97 - 19 = ?', N'78', N'77', N'80', N'83', 'A'),
(N'Lớp 5', N'Toán', N'[Lớp 5 - Toán] Kết quả của 12 x 5 là bao nhiêu?', N'48', N'60', N'65', N'64', 'B'),
(N'Lớp 5', N'Toán', N'[Lớp 5 - Toán] Tính: 32 : 4 = ?', N'7', N'10', N'8', N'12', 'C'),
(N'Lớp 5', N'Toán', N'[Lớp 5 - Toán] Hình chữ nhật có chiều dài 11 cm và chiều rộng 5 cm. Chu vi là bao nhiêu?', N'16 cm', N'36 cm', N'30 cm', N'32 cm', 'D'),
(N'Lớp 5', N'Toán', N'[Lớp 5 - Toán] Hình chữ nhật dài 12 cm, rộng 5 cm. Diện tích là:', N'60 cm2', N'17 cm2', N'65 cm2', N'48 cm2', 'A'),
(N'Lớp 5', N'Toán', N'[Lớp 5 - Toán] Số tiếp theo của dãy 17, 19, 21, ... là số nào?', N'22', N'23', N'25', N'27', 'B'),
(N'Lớp 5', N'Toán', N'[Lớp 5 - Toán] Mỗi túi có 4 viên bi. Có 6 túi và thêm 5 viên bi rời. Tất cả có bao nhiêu viên bi?', N'23', N'33', N'29', N'27', 'C'),
(N'Lớp 5', N'Toán', N'[Lớp 5 - Toán] Tính: 44 + 16 = ?', N'58', N'63', N'66', N'60', 'D'),
(N'Lớp 5', N'Tiếng Việt', N'[Lớp 5 - Tiếng Việt] Từ nào trái nghĩa với ''gần''?', N'xa', N'mỏng', N'lạnh', N'cuối', 'A'),
(N'Lớp 5', N'Tiếng Việt', N'[Lớp 5 - Tiếng Việt] Từ nào đồng nghĩa với ''im lặng''?', N'lạnh', N'yên tĩnh', N'cuối', N'dưới', 'B'),
(N'Lớp 5', N'Tiếng Việt', N'[Lớp 5 - Tiếng Việt] Từ nào trái nghĩa với ''vui''?', N'cuối', N'dưới', N'buồn', N'ngoài', 'C'),
(N'Lớp 5', N'Tiếng Việt', N'[Lớp 5 - Tiếng Việt] Từ nào đồng nghĩa với ''chính xác''?', N'dưới', N'ngoài', N'giả', N'đúng đắn', 'D'),
(N'Lớp 5', N'Tiếng Việt', N'[Lớp 5 - Tiếng Việt] Từ nào trái nghĩa với ''dày''?', N'mỏng', N'ngoài', N'giả', N'cũ', 'A'),
(N'Lớp 5', N'Tiếng Việt', N'[Lớp 5 - Tiếng Việt] Từ nào đồng nghĩa với ''kiên trì''?', N'giả', N'bền bỉ', N'cũ', N'sai', 'B'),
(N'Lớp 5', N'Tiếng Việt', N'[Lớp 5 - Tiếng Việt] Từ nào trái nghĩa với ''đầu''?', N'cũ', N'sai', N'cuối', N'hẹp', 'C'),
(N'Lớp 5', N'Tiếng Việt', N'[Lớp 5 - Tiếng Việt] Từ nào đồng nghĩa với ''nhỏ bé''?', N'sai', N'hẹp', N'bẩn', N'bé xíu', 'D'),
(N'Lớp 5', N'Tiếng Anh', N'[Lớp 5 - Tiếng Anh] Nghĩa tiếng Việt của từ ''yellow'' là gì?', N'màu vàng', N'quyển vở', N'cái ghế', N'cái bàn', 'A'),
(N'Lớp 5', N'Tiếng Anh', N'[Lớp 5 - Tiếng Anh] Từ tiếng Anh nào có nghĩa là ''gia đình''?', N'chair', N'family', N'table', N'food', 'B'),
(N'Lớp 5', N'Tiếng Anh', N'[Lớp 5 - Tiếng Anh] Nghĩa tiếng Việt của từ ''friend'' là gì?', N'cái bàn', N'thức ăn', N'bạn bè', N'sữa', 'C'),
(N'Lớp 5', N'Tiếng Anh', N'[Lớp 5 - Tiếng Anh] Từ tiếng Anh nào có nghĩa là ''ngôi nhà''?', N'food', N'milk', N'rain', N'house', 'D'),
(N'Lớp 5', N'Tiếng Anh', N'[Lớp 5 - Tiếng Anh] Nghĩa tiếng Việt của từ ''classroom'' là gì?', N'lớp học', N'sữa', N'mưa', N'gió', 'A'),
(N'Lớp 5', N'Tiếng Anh', N'[Lớp 5 - Tiếng Anh] Từ tiếng Anh nào có nghĩa là ''bút chì''?', N'rain', N'pencil', N'wind', N'river', 'B'),
(N'Lớp 5', N'Tiếng Anh', N'[Lớp 5 - Tiếng Anh] Nghĩa tiếng Việt của từ ''notebook'' là gì?', N'gió', N'dòng sông', N'quyển vở', N'ngọn núi', 'C'),
(N'Lớp 5', N'Tiếng Anh', N'[Lớp 5 - Tiếng Anh] Từ tiếng Anh nào có nghĩa là ''cái ghế''?', N'river', N'mountain', N'book', N'chair', 'D');

    IF (SELECT COUNT(1) FROM #SeedQuestions) <> 100
    BEGIN
        THROW 51002, N'So luong seed khong bang 100.', 1;
    END;

    IF EXISTS (
        SELECT 1
        FROM #SeedQuestions
        WHERE CorrectOption NOT IN ('A', 'B', 'C', 'D')
    )
    BEGIN
        THROW 51003, N'CorrectOption khong hop le.', 1;
    END;

    IF OBJECT_ID('tempdb..#InsertedQuestions') IS NOT NULL DROP TABLE #InsertedQuestions;
    CREATE TABLE #InsertedQuestions (
        SeedId INT NOT NULL PRIMARY KEY,
        MACAUHOI INT NOT NULL,
        MAMONHOC INT NOT NULL
    );

    MERGE INTO CAUHOI AS C
    USING (
        SELECT
            S.SeedId,
            S.QuestionText,
            T.MABAIHOC,
            T.MAMONHOC
        FROM #SeedQuestions S
        INNER JOIN #TargetLessons T
            ON T.TENLOP = S.TENLOP
           AND T.TENMONHOC = S.TENMONHOC
    ) AS X
    ON 1 = 0
    WHEN NOT MATCHED THEN
        INSERT (NOIDUNGCAUHOI, MABAIHOC)
        VALUES (X.QuestionText, X.MABAIHOC)
    OUTPUT
        X.SeedId,
        inserted.MACAUHOI,
        X.MAMONHOC
    INTO #InsertedQuestions (SeedId, MACAUHOI, MAMONHOC);

    IF (SELECT COUNT(1) FROM #InsertedQuestions) <> 100
    BEGIN
        THROW 51004, N'Khong insert du 100 cau hoi vao CAUHOI.', 1;
    END;

    INSERT INTO DAPAN (NOIDUNGDAPAN, DUNGLSAI, MAMONHOC, MACAUHOI)
    SELECT
        V.AnswerText,
        CASE WHEN V.AnswerKey = S.CorrectOption THEN 1 ELSE 0 END,
        I.MAMONHOC,
        I.MACAUHOI
    FROM #SeedQuestions S
    INNER JOIN #InsertedQuestions I
        ON I.SeedId = S.SeedId
    CROSS APPLY (VALUES
        ('A', S.OptionA),
        ('B', S.OptionB),
        ('C', S.OptionC),
        ('D', S.OptionD)
    ) V(AnswerKey, AnswerText);

    IF (
        SELECT COUNT(1)
        FROM DAPAN D
        INNER JOIN #InsertedQuestions I ON I.MACAUHOI = D.MACAUHOI
    ) <> 400
    BEGIN
        THROW 51005, N'Khong insert du 400 dap an vao DAPAN.', 1;
    END;

    IF EXISTS (
        SELECT D.MACAUHOI
        FROM DAPAN D
        INNER JOIN #InsertedQuestions I ON I.MACAUHOI = D.MACAUHOI
        GROUP BY D.MACAUHOI
        HAVING COUNT(1) <> 4
            OR SUM(CASE WHEN D.DUNGLSAI = 1 THEN 1 ELSE 0 END) <> 1
    )
    BEGIN
        THROW 51006, N'Moi cau hoi phai co dung 4 dap an va dung 1 dap an dung.', 1;
    END;

    DECLARE @PostActiveCauHoi INT = (SELECT COUNT(1) FROM CAUHOI WHERE IsDeleted = 0);
    DECLARE @PostActiveDapAn INT = (SELECT COUNT(1) FROM DAPAN WHERE IsDeleted = 0);
    DECLARE @PostActiveNonPlaceholderCauHoi INT = (
        SELECT COUNT(1)
        FROM CAUHOI
        WHERE IsDeleted = 0
          AND NOIDUNGCAUHOI NOT LIKE N'Nội dung câu hỏi rèn luyện số % là gì?'
    );
    DECLARE @PostActiveNonPlaceholderDapAn INT = (
        SELECT COUNT(1)
        FROM DAPAN D
        INNER JOIN CAUHOI C ON C.MACAUHOI = D.MACAUHOI
        WHERE D.IsDeleted = 0
          AND C.IsDeleted = 0
          AND C.NOIDUNGCAUHOI NOT LIKE N'Nội dung câu hỏi rèn luyện số % là gì?'
          AND D.NOIDUNGDAPAN NOT LIKE N'Đáp án % cho câu hỏi %'
    );

    IF (@PostActiveNonPlaceholderCauHoi - @PreActiveNonPlaceholderCauHoi) <> 100
    BEGIN
        THROW 51007, N'So cau hoi non-placeholder active tang khong dung +100.', 1;
    END;

    IF (@PostActiveNonPlaceholderDapAn - @PreActiveNonPlaceholderDapAn) <> 400
    BEGIN
        THROW 51008, N'So dap an non-placeholder active tang khong dung +400.', 1;
    END;

    IF EXISTS (
        SELECT 1
        FROM CAUHOI
        WHERE IsDeleted = 0
          AND NOIDUNGCAUHOI LIKE N'Nội dung câu hỏi rèn luyện số % là gì?'
    )
    BEGIN
        THROW 51009, N'Van con cau hoi placeholder o trang thai active.', 1;
    END;

    COMMIT;

    PRINT N'Seed thanh cong: 100 cau hoi moi, 400 dap an moi. Placeholder da duoc soft-delete.';
    PRINT N'Active truoc/sau - CAUHOI: ' + CAST(@PreActiveCauHoi AS NVARCHAR(20)) + N' -> ' + CAST(@PostActiveCauHoi AS NVARCHAR(20));
    PRINT N'Active truoc/sau - DAPAN: ' + CAST(@PreActiveDapAn AS NVARCHAR(20)) + N' -> ' + CAST(@PostActiveDapAn AS NVARCHAR(20));
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK;
    THROW;
END CATCH;
GO
