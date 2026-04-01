SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    IF OBJECT_ID('tempdb..#TargetLessons') IS NOT NULL DROP TABLE #TargetLessons;
    CREATE TABLE #TargetLessons (
        MABAIHOC INT NOT NULL PRIMARY KEY,
        TENLOP NVARCHAR(50) NOT NULL,
        TENMONHOC NVARCHAR(50) NOT NULL
    );

    INSERT INTO #TargetLessons (MABAIHOC, TENLOP, TENMONHOC)
    SELECT
        C.MABAIHOC,
        L.TENLOP,
        M.TENMONHOC
    FROM CAUHOI C
    INNER JOIN BAIHOC B ON B.MABAIHOC = C.MABAIHOC
    INNER JOIN LOP L ON L.MALOP = B.MALOP
    INNER JOIN MONHOC M ON M.MAMONHOC = B.MAMONHOC
    WHERE C.IsDeleted = 0
      AND C.NOIDUNGCAUHOI LIKE N'[[]Lớp %'
    GROUP BY C.MABAIHOC, L.TENLOP, M.TENMONHOC;

    IF (SELECT COUNT(1) FROM #TargetLessons) <> 12
    BEGIN
        THROW 52001, N'Khong tim thay du 12 bai hoc dang chua bo cau hoi moi.', 1;
    END;

    IF OBJECT_ID('tempdb..#LessonContent') IS NOT NULL DROP TABLE #LessonContent;
    CREATE TABLE #LessonContent (
        TENLOP NVARCHAR(50) NOT NULL,
        TENMONHOC NVARCHAR(50) NOT NULL,
        NOIDUNGLYTUYET NVARCHAR(MAX) NOT NULL,
        MEOGHINHO NVARCHAR(MAX) NOT NULL,
        PRIMARY KEY (TENLOP, TENMONHOC)
    );

    INSERT INTO #LessonContent (TENLOP, TENMONHOC, NOIDUNGLYTUYET, MEOGHINHO)
    VALUES
    (
        N'Lớp 2',
        N'Toán',
        N'Mục tiêu: luyện cộng trừ trong phạm vi 100 và giải toán có lời văn một bước. Trọng tâm: nhận diện dữ kiện, chọn phép tính phù hợp, trình bày phép tính rõ ràng. Học sinh luyện so sánh số, tính nhẩm nhanh và kiểm tra lại kết quả bằng phép tính ngược.',
        N'Mẹo ghi nhớ: Đọc kỹ đề 2 lần, gạch chân số liệu chính, làm phép tính rồi thử ngược lại để tự kiểm tra.'
    ),
    (
        N'Lớp 2',
        N'Tiếng Việt',
        N'Mục tiêu: nhận biết từ chỉ sự vật, hoạt động, đặc điểm và đặt câu đơn đúng mẫu. Trọng tâm: dùng từ đúng ngữ cảnh, viết câu đủ chủ ngữ và vị ngữ, ngắt nghỉ đúng dấu câu cơ bản. Học sinh luyện mở rộng vốn từ theo chủ điểm gia đình và trường lớp.',
        N'Mẹo ghi nhớ: Câu rõ ý thường trả lời được Ai hoặc Cái gì làm gì hoặc như thế nào.'
    ),
    (
        N'Lớp 2',
        N'Tiếng Anh',
        N'Mục tiêu: ghi nhớ từ vựng quen thuộc về lớp học, gia đình, màu sắc và đồ vật. Trọng tâm: mẫu câu đơn giản với This is, That is, I have, I like và câu hỏi ngắn What is this. Học sinh ưu tiên nghe đúng phát âm và phản xạ từ theo tranh.',
        N'Mẹo ghi nhớ: Học theo cặp từ với hình ảnh, nói thành câu ngắn mỗi khi thấy đồ vật xung quanh.'
    ),
    (
        N'Lớp 3',
        N'Toán',
        N'Mục tiêu: thành thạo phép nhân chia trong phạm vi đã học và vận dụng vào bài toán thực tế. Trọng tâm: thuộc bảng nhân chia, hiểu ý nghĩa chia đều và chia theo nhóm, tính chu vi hình đơn giản. Học sinh rèn kỹ năng chọn phép tính từ dữ kiện lời văn.',
        N'Mẹo ghi nhớ: Nhìn từ khóa chia đều hoặc mỗi nhóm để chọn phép chia, từ khóa tất cả để nghĩ đến phép nhân hoặc cộng.'
    ),
    (
        N'Lớp 3',
        N'Tiếng Việt',
        N'Mục tiêu: phân biệt từ đồng nghĩa, trái nghĩa và dùng đúng dấu câu trong đoạn văn ngắn. Trọng tâm: chọn từ phù hợp sắc thái, liên kết câu mạch lạc, tránh lặp từ không cần thiết. Học sinh luyện viết đoạn 3 đến 5 câu có mở ý và kết ý.',
        N'Mẹo ghi nhớ: Viết xong hãy đọc to từng câu, chỗ nào khó đọc thường là chỗ cần sửa từ hoặc dấu câu.'
    ),
    (
        N'Lớp 3',
        N'Tiếng Anh',
        N'Mục tiêu: mở rộng từ vựng chủ đề trường học, hoạt động thường ngày và thời gian trong ngày. Trọng tâm: câu khẳng định, phủ định, nghi vấn hiện tại đơn ở mức cơ bản; dùng do và does trong câu hỏi đơn giản. Học sinh luyện trả lời ngắn Yes I do hoặc No I do not.',
        N'Mẹo ghi nhớ: Câu hỏi với he she thì nghĩ đến does, còn I you we they thì dùng do.'
    ),
    (
        N'Lớp 4',
        N'Toán',
        N'Mục tiêu: nắm chắc phân số, phép tính với số tự nhiên và giải bài toán nhiều bước. Trọng tâm: hiểu tử số mẫu số, so sánh phân số cùng mẫu hoặc cùng tử, tìm thành phần chưa biết trong phép tính. Học sinh rèn trình bày từng bước để tránh sai sót.',
        N'Mẹo ghi nhớ: Khi làm phân số, so sánh mẫu trước rồi mới xét tử để chọn cách làm nhanh.'
    ),
    (
        N'Lớp 4',
        N'Tiếng Việt',
        N'Mục tiêu: xác định chủ ngữ, vị ngữ, trạng ngữ và viết đoạn văn có liên kết ý rõ ràng. Trọng tâm: dùng câu kể, câu hỏi, câu cảm đúng mục đích; thay từ để tránh lặp; dùng từ nối hợp lý. Học sinh luyện tóm tắt và diễn đạt lại nội dung bằng lời của mình.',
        N'Mẹo ghi nhớ: Mỗi đoạn chỉ nên giữ một ý chính, các câu sau phải làm rõ cho ý chính đó.'
    ),
    (
        N'Lớp 4',
        N'Tiếng Anh',
        N'Mục tiêu: sử dụng câu mô tả thói quen, vị trí đồ vật và đặc điểm quen thuộc. Trọng tâm: mẫu câu There is, There are; giới từ chỉ vị trí; so sánh hơn với tính từ ngắn ở mức cơ bản. Học sinh luyện đặt câu theo tranh để tăng phản xạ ngôn ngữ.',
        N'Mẹo ghi nhớ: Nói vị trí theo cụm hoàn chỉnh như on the table hoặc under the chair thay vì nói rời từng từ.'
    ),
    (
        N'Lớp 5',
        N'Toán',
        N'Mục tiêu: vận dụng số thập phân, tỉ số phần trăm và công thức diện tích chu vi trong bài toán thực tế. Trọng tâm: đổi đơn vị đúng thứ tự, đọc kỹ điều kiện bài toán, kiểm tra tính hợp lý của kết quả. Học sinh luyện giải bài nhiều bước và trình bày ngắn gọn.',
        N'Mẹo ghi nhớ: Với bài phần trăm, luôn xác định rõ giá trị gốc 100 phần trước khi tính.'
    ),
    (
        N'Lớp 5',
        N'Tiếng Việt',
        N'Mục tiêu: nâng cao kỹ năng đọc hiểu, dùng từ chính xác và viết đoạn văn giàu liên kết. Trọng tâm: nhận biết từ nhiều nghĩa, từ đồng âm, biện pháp so sánh và nhân hóa ở mức phù hợp. Học sinh luyện viết đoạn văn có câu mở đoạn, triển khai và câu kết đoạn.',
        N'Mẹo ghi nhớ: Trước khi nộp bài, rà lại ba điểm: đúng ý đề, đúng chính tả, đúng dấu câu.'
    ),
    (
        N'Lớp 5',
        N'Tiếng Anh',
        N'Mục tiêu: củng cố ngữ pháp nền tảng để giao tiếp và làm bài đọc hiểu ngắn. Trọng tâm: hiện tại đơn, hiện tại tiếp diễn ở mức cơ bản, trạng từ chỉ tần suất và câu hỏi thông tin quen thuộc. Học sinh luyện viết câu hoàn chỉnh theo mẫu chủ ngữ động từ bổ ngữ.',
        N'Mẹo ghi nhớ: Mỗi câu tiếng Anh cần đủ chủ ngữ và động từ, thiếu một trong hai là câu chưa hoàn chỉnh.'
    );

    IF EXISTS (
        SELECT 1
        FROM #TargetLessons T
        LEFT JOIN #LessonContent C
            ON C.TENLOP = T.TENLOP
           AND C.TENMONHOC = T.TENMONHOC
        WHERE C.TENLOP IS NULL
    )
    BEGIN
        THROW 52002, N'Thieu noi dung cho it nhat 1 cap Lop Mon.', 1;
    END;

    UPDATE B
    SET
        B.NOIDUNGLYTUYET = C.NOIDUNGLYTUYET,
        B.MEOGHINHO = C.MEOGHINHO
    FROM BAIHOC B
    INNER JOIN #TargetLessons T ON T.MABAIHOC = B.MABAIHOC
    INNER JOIN #LessonContent C
        ON C.TENLOP = T.TENLOP
       AND C.TENMONHOC = T.TENMONHOC;

    IF @@ROWCOUNT <> 12
    BEGIN
        THROW 52003, N'So dong duoc update trong BAIHOC khong bang 12.', 1;
    END;

    COMMIT;
    PRINT N'Update BAIHOC thanh cong: da dong bo NOI DUNG LY THUYET va MEO GHI NHO cho 12 bai hoc.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK;
    THROW;
END CATCH;
GO
