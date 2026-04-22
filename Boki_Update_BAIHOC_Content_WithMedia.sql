SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    IF OBJECT_ID('tempdb..#TargetLessons') IS NOT NULL
        DROP TABLE #TargetLessons;

    CREATE TABLE #TargetLessons
    (
        MABAIHOC INT NOT NULL PRIMARY KEY,
        TENLOP NVARCHAR(100) NOT NULL,
        MAMONHOC INT NOT NULL,
        SOBAI INT NOT NULL
    );

    INSERT INTO #TargetLessons (MABAIHOC, TENLOP, MAMONHOC, SOBAI)
    SELECT
        B.MABAIHOC,
        L.TENLOP,
        B.MAMONHOC,
        CASE WHEN B.TENBAIHOC LIKE N'%2:%' THEN 2 ELSE 1 END
    FROM BAIHOC B
    INNER JOIN LOP L ON L.MALOP = B.MALOP
    WHERE B.IsDeleted = 0
      AND B.MALOP IN (1, 2, 3, 4)
      AND B.MAMONHOC IN (1, 2, 3);

    DECLARE @TargetCount INT = (SELECT COUNT(1) FROM #TargetLessons);
    IF @TargetCount = 0
    BEGIN
        THROW 53001, N'Khong tim thay bai hoc de cap nhat noi dung media.', 1;
    END;

    UPDATE B
    SET
        B.NOIDUNGLYTUYET =
            CASE
                WHEN T.MAMONHOC = 1 AND T.SOBAI = 1 THEN
                    N'<h4>Muc tieu bai hoc (' + T.TENLOP + N')</h4>
<p>Hoc sinh cuong co phep tinh co ban, doc de dung trong bai toan co loi van, va trinh bay tung buoc ro rang.</p>
<p>Phan luyen tap tap trung vao cach nhan dien du kien, chon phep tinh phu hop, va tu kiem tra ket qua sau khi giai.</p>
<p>Co the cho hoc sinh lam theo nhom 2-3 ban: moi nhom tom tat de, de xuat cach giai, sau do doi cheo bai de kiem tra.</p>
<p>Goi y media minh hoa de tao hung thu khi vao bai.</p>
<figure><img src="https://dummyimage.com/960x540/e8f0ff/1b3b7a.png&text=Toan+' + REPLACE(T.TENLOP, N' ', N'+') + N'+Bai+1" alt="Toan minh hoa"/></figure>
<video controls preload="metadata"><source src="https://www.w3schools.com/html/mov_bbb.mp4" type="video/mp4" /></video>'
                WHEN T.MAMONHOC = 1 AND T.SOBAI = 2 THEN
                    N'<h4>Van dung bai toan nhieu buoc (' + T.TENLOP + N')</h4>
<p>Hoc sinh ren ky nang tach bai toan thanh nhieu buoc ngan, tim thu tu thuc hien hop ly, va ghi dap an day du.</p>
<p>Phan bai tap ket hop doi don vi, uoc luong ket qua, va so sanh hai cach giai de tim cach ngan gon nhung chinh xac.</p>
<p>Khuyen khich hoc sinh dung so do doan thang hoac bang tom tat de tranh bo sot du kien quan trong.</p>
<p>Media kem theo giup hoc sinh quan sat tinh huong thuc te truoc khi vao phan bai tap.</p>
<figure><img src="https://dummyimage.com/960x540/dfefff/0f4fb8.png&text=Toan+' + REPLACE(T.TENLOP, N' ', N'+') + N'+Bai+2" alt="Toan bai 2"/></figure>
<video controls preload="metadata"><source src="https://samplelib.com/lib/preview/mp4/sample-5s.mp4" type="video/mp4" /></video>'
                WHEN T.MAMONHOC = 2 AND T.SOBAI = 1 THEN
                    N'<h4>Doc - hieu va dat cau (' + T.TENLOP + N')</h4>
<p>Hoc sinh mo rong von tu theo chu de gan gui, biet dat cau dung mau va dung dau cau phu hop voi ngu canh.</p>
<p>Trong gio hoc, giao vien co the cho hoc sinh doc to theo cap, sau do thao luan nhanh ve tu khoa chinh cua doan van.</p>
<p>Phan thuc hanh uu tien viet 3-5 cau ngan co lien ket y, dung tu chinh xac, va han che lap tu khong can thiet.</p>
<p>Media minh hoa tao boi canh ngon ngu de hoc sinh de hinh dung noi dung bai.</p>
<figure><img src="https://dummyimage.com/960x540/fff2e2/8d4f00.png&text=Tieng+Viet+' + REPLACE(T.TENLOP, N' ', N'+') + N'+Bai+1" alt="Tieng Viet minh hoa"/></figure>
<video controls preload="metadata"><source src="https://interactive-examples.mdn.mozilla.net/media/cc0-videos/flower.mp4" type="video/mp4" /></video>'
                WHEN T.MAMONHOC = 2 AND T.SOBAI = 2 THEN
                    N'<h4>Viet doan van co lien ket (' + T.TENLOP + N')</h4>
<p>Hoc sinh luyen cach lap dan y ngan, viet cau mo doan, cau trien khai, va cau ket doan theo mot chu de cu the.</p>
<p>Trong bai hoc nay, hoc sinh duoc huong dan cach thay the tu dong nghia de doan van tu nhien va de doc hon.</p>
<p>Giao vien co the dung hoat dong nhan xet theo nhom: moi nhom doc bai cua ban, danh dau cau manh va cau can sua.</p>
<p>Media minh hoa duoc chen truc tiep de hoc sinh ket hop quan sat va viet.</p>
<figure><img src="https://dummyimage.com/960x540/ffe9d6/a34b00.png&text=Tieng+Viet+' + REPLACE(T.TENLOP, N' ', N'+') + N'+Bai+2" alt="Tieng Viet bai 2"/></figure>
<video controls preload="metadata"><source src="https://www.w3schools.com/html/movie.mp4" type="video/mp4" /></video>'
                WHEN T.MAMONHOC = 3 AND T.SOBAI = 1 THEN
                    N'<h4>Tu vung va mau cau co ban (' + T.TENLOP + N')</h4>
<p>Hoc sinh hoc tu vung theo chu de quen thuoc: school, family, hobby; dong thoi luyen phat am va nhan trong am tu.</p>
<p>Phan cau truc tap trung vao mau cau ngan: This is..., I have..., I like..., giup hoc sinh tu tin khi noi.</p>
<p>Hoat dong de xuat: giao tiep cap doi, moi ban dat 3 cau hoi ngan va doi ban tra loi bang cau day du.</p>
<p>Media minh hoa duoc chen trong bai de tang kha nang ghi nho tu vung.</p>
<figure><img src="https://dummyimage.com/960x540/e7f7ff/0a4e73.png&text=Tieng+Anh+' + REPLACE(T.TENLOP, N' ', N'+') + N'+Bai+1" alt="Tieng Anh minh hoa"/></figure>
<video controls preload="metadata"><source src="https://samplelib.com/lib/preview/mp4/sample-10s.mp4" type="video/mp4" /></video>'
                WHEN T.MAMONHOC = 3 AND T.SOBAI = 2 THEN
                    N'<h4>Phan xa giao tiep va ngu phap co ban (' + T.TENLOP + N')</h4>
<p>Hoc sinh ren cau hoi - dap ngan voi thi hien tai don, trang tu tan suat, va cau truc mo ta vi tri vat dung.</p>
<p>Muc tieu chinh la noi cau ro y, du chu ngu va dong tu, ket hop tu vung da hoc vao tinh huong giao tiep thuc te.</p>
<p>Giao vien co the to chuc mini role-play: mua sam, gioi thieu ban than, hoi thoi gian bieu trong ngay.</p>
<p>Media minh hoa giup hoc sinh nghe nhin va lap cau nhanh hon.</p>
<figure><img src="https://dummyimage.com/960x540/dcf3ff/0d5f85.png&text=Tieng+Anh+' + REPLACE(T.TENLOP, N' ', N'+') + N'+Bai+2" alt="Tieng Anh bai 2"/></figure>
<video controls preload="metadata"><source src="https://interactive-examples.mdn.mozilla.net/media/cc0-videos/flower.webm" type="video/webm" /></video>'
                ELSE B.NOIDUNGLYTUYET
            END,
        B.MEOGHINHO =
            CASE
                WHEN T.MAMONHOC = 1 THEN
                    N'<h5>Meo ghi nho nhanh</h5><ul><li>Doc de 2 lan va gach chan du kien chinh.</li><li>Tom tat de truoc khi dat phep tinh.</li><li>Sau khi giai xong, doi nguoc phep tinh de tu kiem tra.</li></ul>'
                WHEN T.MAMONHOC = 2 THEN
                    N'<h5>Meo ghi nho nhanh</h5><ul><li>Moi cau nen ro ai/cai gi, lam gi, nhu the nao.</li><li>Doc lai doan van thanh tieng de phat hien loi.</li><li>Moi doan giu mot y chinh ro rang.</li></ul>'
                WHEN T.MAMONHOC = 3 THEN
                    N'<h5>Meo ghi nho nhanh</h5><ul><li>Moi cau tieng Anh can du chu ngu va dong tu.</li><li>Hoc tu vung theo cap: tu + cau vi du.</li><li>On bai theo nhom cau hoi-ngan de tang phan xa.</li></ul>'
                ELSE B.MEOGHINHO
            END
    FROM BAIHOC B
    INNER JOIN #TargetLessons T ON T.MABAIHOC = B.MABAIHOC;

    IF @@ROWCOUNT <> @TargetCount
    BEGIN
        THROW 53002, N'So dong cap nhat khong khop voi danh sach bai hoc muc tieu.', 1;
    END;

    COMMIT;
    PRINT N'Update thanh cong NOI DUNG LY THUYET + MEO GHI NHO co media cho bai hoc.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK;
    THROW;
END CATCH;
GO
