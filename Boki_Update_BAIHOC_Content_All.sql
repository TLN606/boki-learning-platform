SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    IF OBJECT_ID('tempdb..#TargetLessons') IS NOT NULL DROP TABLE #TargetLessons;
    CREATE TABLE #TargetLessons (
        MABAIHOC INT NOT NULL PRIMARY KEY,
        TENLOP NVARCHAR(50) NOT NULL,
        TENMONHOC NVARCHAR(50) NOT NULL,
        SOBAI INT NOT NULL
    );

    INSERT INTO #TargetLessons (MABAIHOC, TENLOP, TENMONHOC, SOBAI)
    SELECT
        B.MABAIHOC,
        L.TENLOP,
        M.TENMONHOC,
        CASE
            WHEN B.TENBAIHOC LIKE N'Bài số 2:%' THEN 2
            ELSE 1
        END AS SOBAI
    FROM BAIHOC B
    INNER JOIN LOP L ON L.MALOP = B.MALOP
    INNER JOIN MONHOC M ON M.MAMONHOC = B.MAMONHOC
    WHERE B.IsDeleted = 0
      AND L.TENLOP IN (N'Lớp 2', N'Lớp 3', N'Lớp 4', N'Lớp 5')
      AND M.TENMONHOC IN (N'Toán', N'Tiếng Việt', N'Tiếng Anh');

    DECLARE @TargetCount INT = (SELECT COUNT(1) FROM #TargetLessons);
    IF @TargetCount < 12
    BEGIN
        THROW 52101, N'So bai hoc muc tieu qua it, dung script de tranh update sai.', 1;
    END;

    UPDATE B
    SET
        B.NOIDUNGLYTUYET =
            CASE
                WHEN T.TENMONHOC = N'Toán' AND T.SOBAI = 1 THEN
                    N'Mục tiêu ' + T.TENLOP + N': củng cố phép tính cơ bản, đọc đúng dữ kiện và giải bài toán có lời văn theo từng bước. Trọng tâm: xác định dữ kiện quan trọng, chọn phép tính phù hợp, trình bày phép tính rõ ràng và kiểm tra kết quả bằng phép tính ngược.'
                WHEN T.TENMONHOC = N'Toán' AND T.SOBAI = 2 THEN
                    N'Mục tiêu ' + T.TENLOP + N': mở rộng vận dụng vào bài toán thực tế và bài toán nhiều bước. Trọng tâm: ước lượng kết quả, đổi đơn vị hợp lý, so sánh các cách giải và chọn cách ngắn gọn nhưng chính xác.'
                WHEN T.TENMONHOC = N'Tiếng Việt' AND T.SOBAI = 1 THEN
                    N'Mục tiêu ' + T.TENLOP + N': phát triển vốn từ và nhận diện đúng kiểu câu cơ bản. Trọng tâm: dùng từ đúng ngữ cảnh, viết câu đủ thành phần, ngắt nghỉ theo dấu câu và liên kết ý trong đoạn văn ngắn.'
                WHEN T.TENMONHOC = N'Tiếng Việt' AND T.SOBAI = 2 THEN
                    N'Mục tiêu ' + T.TENLOP + N': nâng cao đọc hiểu và diễn đạt mạch lạc theo chủ đề. Trọng tâm: phân tích từ đồng nghĩa trái nghĩa hoặc từ nhiều nghĩa, viết đoạn có câu mở ý và kết ý, hạn chế lặp từ.'
                WHEN T.TENMONHOC = N'Tiếng Anh' AND T.SOBAI = 1 THEN
                    N'Mục tiêu ' + T.TENLOP + N': ghi nhớ từ vựng nền tảng và mẫu câu giao tiếp ngắn hằng ngày. Trọng tâm: phát âm đúng từ quen thuộc, trả lời câu hỏi đơn giản, dùng mẫu câu khẳng định và nghi vấn cơ bản.'
                WHEN T.TENMONHOC = N'Tiếng Anh' AND T.SOBAI = 2 THEN
                    N'Mục tiêu ' + T.TENLOP + N': mở rộng cấu trúc câu và tăng phản xạ giao tiếp theo tình huống lớp học. Trọng tâm: dùng đúng thì cơ bản, trạng từ chỉ tần suất và cụm từ chỉ vị trí, viết câu hoàn chỉnh có chủ ngữ và động từ.'
                ELSE B.NOIDUNGLYTUYET
            END,
        B.MEOGHINHO =
            CASE
                WHEN T.TENMONHOC = N'Toán' AND T.SOBAI = 1 THEN
                    N'Mẹo ghi nhớ: Đọc đề hai lần, gạch chân dữ kiện chính, làm xong thử lại bằng phép tính ngược.'
                WHEN T.TENMONHOC = N'Toán' AND T.SOBAI = 2 THEN
                    N'Mẹo ghi nhớ: Với bài nhiều bước, ghi tóm tắt ngắn theo thứ tự trước rồi mới bấm phép tính.'
                WHEN T.TENMONHOC = N'Tiếng Việt' AND T.SOBAI = 1 THEN
                    N'Mẹo ghi nhớ: Mỗi câu nên trả lời được ai hoặc cái gì làm gì hoặc như thế nào.'
                WHEN T.TENMONHOC = N'Tiếng Việt' AND T.SOBAI = 2 THEN
                    N'Mẹo ghi nhớ: Viết xong đoạn văn hãy đọc to một lượt để tự sửa từ và dấu câu chưa hợp lý.'
                WHEN T.TENMONHOC = N'Tiếng Anh' AND T.SOBAI = 1 THEN
                    N'Mẹo ghi nhớ: Học từ theo cặp hình ảnh và nói thành câu ngắn ngay sau khi nhớ từ.'
                WHEN T.TENMONHOC = N'Tiếng Anh' AND T.SOBAI = 2 THEN
                    N'Mẹo ghi nhớ: Mỗi câu tiếng Anh luôn cần đủ chủ ngữ và động từ trước khi thêm thông tin khác.'
                ELSE B.MEOGHINHO
            END
    FROM BAIHOC B
    INNER JOIN #TargetLessons T ON T.MABAIHOC = B.MABAIHOC;

    IF @@ROWCOUNT <> @TargetCount
    BEGIN
        THROW 52102, N'So dong update khong trung voi so bai hoc muc tieu.', 1;
    END;

    COMMIT;
    PRINT N'Update thanh cong toan bo BAIHOC trong pham vi Lop 2-5, Toan/Tieng Viet/Tieng Anh.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK;
    THROW;
END CATCH;
GO
