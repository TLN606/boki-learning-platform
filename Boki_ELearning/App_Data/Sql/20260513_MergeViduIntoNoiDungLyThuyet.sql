IF COL_LENGTH('dbo.BAIHOC', 'VIDU') IS NOT NULL
BEGIN
    UPDATE dbo.BAIHOC
    SET NOIDUNGLYTUYET =
        LTRIM(RTRIM(
            COALESCE(NULLIF(NOIDUNGLYTUYET, N''), N'') +
            CASE
                WHEN NULLIF(NOIDUNGLYTUYET, N'') IS NOT NULL
                     AND NULLIF(VIDU, N'') IS NOT NULL
                THEN CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10)
                ELSE N''
            END +
            COALESCE(NULLIF(VIDU, N''), N'')
        ))
    WHERE NULLIF(VIDU, N'') IS NOT NULL
      AND (
          NOIDUNGLYTUYET IS NULL
          OR CHARINDEX(VIDU, NOIDUNGLYTUYET) = 0
      );
END
GO
