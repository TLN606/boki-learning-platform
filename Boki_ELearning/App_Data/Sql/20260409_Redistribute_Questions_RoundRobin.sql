USE BokiDB;
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    DECLARE @BeforeActiveQuestions INT = (
        SELECT COUNT(1)
        FROM CAUHOI
        WHERE IsDeleted = 0
          AND TRANGTHAI = N'Active'
    );

    IF @BeforeActiveQuestions <> 100
    BEGIN
        THROW 53001, N'So cau hoi Active hien tai khong phai 100, script dung de tranh sai du lieu.', 1;
    END;

    IF OBJECT_ID('tempdb..#LessonPairs') IS NOT NULL DROP TABLE #LessonPairs;
    CREATE TABLE #LessonPairs
    (
        MALOP INT NOT NULL,
        MAMONHOC INT NOT NULL,
        MABAIHOC INT NOT NULL,
        LessonOrder INT NOT NULL,
        PRIMARY KEY (MALOP, MAMONHOC, LessonOrder)
    );

    INSERT INTO #LessonPairs (MALOP, MAMONHOC, MABAIHOC, LessonOrder)
    SELECT
        B.MALOP,
        B.MAMONHOC,
        B.MABAIHOC,
        ROW_NUMBER() OVER (PARTITION BY B.MALOP, B.MAMONHOC ORDER BY B.MABAIHOC)
    FROM BAIHOC B
    WHERE B.IsDeleted = 0
      AND B.MALOP IN (
            SELECT MALOP FROM LOP WHERE TENLOP IN (N'Lớp 2', N'Lớp 3', N'Lớp 4', N'Lớp 5')
      )
      AND B.MAMONHOC IN (
            SELECT MAMONHOC FROM MONHOC WHERE TENMONHOC IN (N'Toán', N'Tiếng Việt', N'Tiếng Anh')
      );

    IF EXISTS (
        SELECT 1
        FROM #LessonPairs
        GROUP BY MALOP, MAMONHOC
        HAVING COUNT(1) <> 2
    )
    BEGIN
        THROW 53002, N'Moi cap Lop/Mon phai co dung 2 bai hoc Active de chia round-robin.', 1;
    END;

    IF OBJECT_ID('tempdb..#QuestionTarget') IS NOT NULL DROP TABLE #QuestionTarget;
    CREATE TABLE #QuestionTarget
    (
        MACAUHOI INT NOT NULL PRIMARY KEY,
        TargetLessonId INT NOT NULL
    );

    ;WITH ActiveQuestions AS
    (
        SELECT
            C.MACAUHOI,
            B.MALOP,
            B.MAMONHOC,
            ROW_NUMBER() OVER (PARTITION BY B.MALOP, B.MAMONHOC ORDER BY C.MACAUHOI) AS rn
        FROM CAUHOI C
        INNER JOIN BAIHOC B ON B.MABAIHOC = C.MABAIHOC
        WHERE C.IsDeleted = 0
          AND C.TRANGTHAI = N'Active'
          AND B.IsDeleted = 0
          AND B.MALOP IN (SELECT MALOP FROM #LessonPairs)
          AND B.MAMONHOC IN (SELECT MAMONHOC FROM #LessonPairs)
    )
    INSERT INTO #QuestionTarget (MACAUHOI, TargetLessonId)
    SELECT
        Q.MACAUHOI,
        P.MABAIHOC
    FROM ActiveQuestions Q
    INNER JOIN #LessonPairs P
        ON P.MALOP = Q.MALOP
       AND P.MAMONHOC = Q.MAMONHOC
       AND P.LessonOrder = ((Q.rn - 1) % 2) + 1;

    IF (SELECT COUNT(1) FROM #QuestionTarget) <> @BeforeActiveQuestions
    BEGIN
        THROW 53003, N'Khong xac dinh du target lesson cho toan bo 100 cau hoi Active.', 1;
    END;

    UPDATE C
    SET C.MABAIHOC = T.TargetLessonId
    FROM CAUHOI C
    INNER JOIN #QuestionTarget T ON T.MACAUHOI = C.MACAUHOI;

    DECLARE @AfterActiveQuestions INT = (
        SELECT COUNT(1)
        FROM CAUHOI
        WHERE IsDeleted = 0
          AND TRANGTHAI = N'Active'
    );

    IF @AfterActiveQuestions <> 100
    BEGIN
        THROW 53004, N'Sau khi update, tong cau hoi Active khong con 100.', 1;
    END;

    IF EXISTS (
        SELECT B.MABAIHOC
        FROM BAIHOC B
        INNER JOIN #LessonPairs P ON P.MABAIHOC = B.MABAIHOC
        LEFT JOIN CAUHOI C
            ON C.MABAIHOC = B.MABAIHOC
           AND C.IsDeleted = 0
           AND C.TRANGTHAI = N'Active'
        GROUP BY B.MABAIHOC
        HAVING COUNT(C.MACAUHOI) = 0
    )
    BEGIN
        THROW 53005, N'Con bai hoc khong co cau hoi Active sau khi chia du lieu.', 1;
    END;

    IF EXISTS (
        SELECT C.MACAUHOI
        FROM CAUHOI C
        LEFT JOIN DAPAN D ON D.MACAUHOI = C.MACAUHOI AND D.IsDeleted = 0
        WHERE C.IsDeleted = 0
          AND C.TRANGTHAI = N'Active'
        GROUP BY C.MACAUHOI
        HAVING COUNT(D.MADAPAN) <> 4
    )
    BEGIN
        THROW 53006, N'Phat hien cau hoi khong du 4 dap an Active sau khi chia du lieu.', 1;
    END;

    COMMIT;

    PRINT N'Hoan tat chia round-robin 100 cau hoi Active qua 24 bai hoc.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK;

    THROW;
END CATCH;
GO
