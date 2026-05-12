-- Thêm cột VIDU nếu chưa có
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME='BAIHOC' AND COLUMN_NAME='VIDU')
  ALTER TABLE BAIHOC ADD VIDU NVARCHAR(MAX) NULL;
GO

-- ========== TOÁN LỚP 2 ==========
UPDATE BAIHOC SET TENBAIHOC=N'Phép cộng trong phạm vi 100',
NOIDUNGLYTUYET=N'Phép cộng là gộp các số lại với nhau. Số hạng + Số hạng = Tổng.
- Cộng không nhớ: cộng từng hàng (đơn vị rồi chục)
- Cộng có nhớ: khi tổng hàng đơn vị ≥ 10, nhớ 1 sang hàng chục',
MEOGHINHO=N'💡 Đặt tính dọc, cộng từ phải sang trái. Hàng đơn vị ≥ 10 thì viết hàng đơn vị, nhớ 1.',
VIDU=N'📝 VD1: 35 + 24 = ?
  35
+ 24
----
  59 (5+4=9, 3+2=5)

📝 VD2: 47 + 36 = ?
  47
+ 36
----
  83 (7+6=13, viết 3 nhớ 1; 4+3+1=8)'
WHERE MABAIHOC=1;

UPDATE BAIHOC SET TENBAIHOC=N'Phép trừ trong phạm vi 100',
NOIDUNGLYTUYET=N'Phép trừ là bớt đi một lượng. Số bị trừ - Số trừ = Hiệu.
- Trừ không nhớ: trừ từng hàng
- Trừ có nhớ: khi chữ số hàng đơn vị không đủ trừ, mượn 1 chục',
MEOGHINHO=N'💡 Kiểm tra bằng phép cộng ngược: Hiệu + Số trừ = Số bị trừ.',
VIDU=N'📝 VD1: 68 - 25 = ?
  68
- 25
----
  43 (8-5=3, 6-2=4)

📝 VD2: 52 - 17 = ?
  52
- 17
----
  35 (2<7 mượn 1: 12-7=5, 5-1-1=3)'
WHERE MABAIHOC=2;

-- ========== TIẾNG VIỆT LỚP 2 ==========
UPDATE BAIHOC SET TENBAIHOC=N'Từ chỉ sự vật, hoạt động, tính chất',
NOIDUNGLYTUYET=N'Ba loại từ cơ bản:
1. Từ chỉ sự vật: tên gọi người, con vật, đồ vật, cây cối (bạn, mèo, bàn, hoa)
2. Từ chỉ hoạt động: hành động, việc làm (chạy, viết, ăn, ngủ)
3. Từ chỉ tính chất: đặc điểm, tính chất (đẹp, cao, xanh, ngọt)',
MEOGHINHO=N'💡 Hỏi "Cái gì/Ai?" → sự vật. "Làm gì?" → hoạt động. "Như thế nào?" → tính chất.',
VIDU=N'📝 Phân loại từ trong câu: "Con mèo trắng đang ngủ."
• mèo → sự vật (con gì?)
• trắng → tính chất (như thế nào?)
• ngủ → hoạt động (làm gì?)'
WHERE MABAIHOC=3;

UPDATE BAIHOC SET TENBAIHOC=N'Dấu chấm, dấu phẩy, dấu chấm hỏi',
NOIDUNGLYTUYET=N'Các dấu câu thường gặp:
- Dấu chấm (.): kết thúc câu kể
- Dấu phẩy (,): ngăn cách các phần trong câu
- Dấu chấm hỏi (?): kết thúc câu hỏi
- Dấu chấm than (!): kết thúc câu cảm thán hoặc cầu khiến',
MEOGHINHO=N'💡 Sau dấu chấm luôn viết hoa chữ cái đầu câu mới.',
VIDU=N'📝 Điền dấu câu:
"Em đi học_ Hôm nay trời đẹp_" → "Em đi học. Hôm nay trời đẹp."
"Bạn tên là gì_" → "Bạn tên là gì?"
"Đẹp quá_" → "Đẹp quá!"'
WHERE MABAIHOC=4;

-- ========== TIẾNG ANH LỚP 2 ==========
UPDATE BAIHOC SET TENBAIHOC=N'Greetings – Chào hỏi',
NOIDUNGLYTUYET=N'Các câu chào cơ bản:
• Hello / Hi – Xin chào
• Good morning – Chào buổi sáng
• Good afternoon – Chào buổi chiều
• Good evening – Chào buổi tối
• How are you? – Bạn khỏe không?
• I am fine, thank you. – Tôi khỏe, cảm ơn.',
MEOGHINHO=N'💡 Morning = sáng, Afternoon = After+Noon = sau trưa, Evening = tối.',
VIDU=N'📝 Hội thoại:
A: "Hello! How are you?"
B: "Hi! I am fine, thank you. And you?"
A: "I am good, thanks!"'
WHERE MABAIHOC=5;

UPDATE BAIHOC SET TENBAIHOC=N'Colors & Numbers – Màu sắc và số đếm',
NOIDUNGLYTUYET=N'Màu sắc: Red (đỏ), Blue (xanh dương), Green (xanh lá), Yellow (vàng), White (trắng), Black (đen), Pink (hồng), Orange (cam).

Số đếm 1-10: One, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten.',
MEOGHINHO=N'💡 Cầu vồng: Red, Orange, Yellow, Green, Blue → nhớ tắt ROYGB.',
VIDU=N'📝 Hỏi đáp:
Q: "What color is the apple?" → A: "It is red."
Q: "How many pencils?" → A: "Three pencils."'
WHERE MABAIHOC=6;

-- ========== TOÁN LỚP 3 ==========
UPDATE BAIHOC SET TENBAIHOC=N'Bảng nhân 2, 3, 4, 5',
NOIDUNGLYTUYET=N'Phép nhân là phép cộng nhiều lần. Thừa số × Thừa số = Tích.
VD: 3 × 4 = 3 + 3 + 3 + 3 = 12

Bảng nhân cần thuộc: nhân 2 (kết quả chẵn), nhân 5 (tận cùng 0 hoặc 5), nhân 10 (thêm 0).',
MEOGHINHO=N'💡 Nhân 2 = gấp đôi. Nhân 5 = nửa của nhân 10. Nhân 9: tổng 2 chữ số = 9.',
VIDU=N'📝 VD1: 4 × 5 = 20 (4+4+4+4+4)
📝 VD2: 3 hộp, mỗi hộp 5 cái bánh → 3 × 5 = 15 cái bánh
📝 VD3: 2 × 8 = 16 (gấp đôi 8)'
WHERE MABAIHOC=7;

UPDATE BAIHOC SET TENBAIHOC=N'Phép chia và chia hết',
NOIDUNGLYTUYET=N'Phép chia là chia đều. Số bị chia ÷ Số chia = Thương.
VD: 12 ÷ 3 = 4 (chia 12 thành 3 phần bằng nhau, mỗi phần 4)

Chia hết: khi phép chia không có dư.
Chia có dư: phần còn lại gọi là số dư (số dư < số chia).',
MEOGHINHO=N'💡 Phép chia ngược với phép nhân: 12÷3=4 vì 4×3=12.',
VIDU=N'📝 VD1: 15 ÷ 5 = 3 (chia 15 kẹo cho 5 bạn, mỗi bạn 3)
📝 VD2: 14 ÷ 3 = 4 dư 2 (4×3=12, 14-12=2)'
WHERE MABAIHOC=8;

-- ========== TIẾNG VIỆT LỚP 3 ==========
UPDATE BAIHOC SET TENBAIHOC=N'Từ đồng nghĩa và trái nghĩa',
NOIDUNGLYTUYET=N'Từ đồng nghĩa: nghĩa giống nhau, thay thế được (xinh đẹp = xinh xắn).
Từ trái nghĩa: nghĩa ngược nhau (to ↔ nhỏ, cao ↔ thấp, nóng ↔ lạnh).',
MEOGHINHO=N'💡 Đồng = giống → đồng nghĩa. Trái = ngược → trái nghĩa.',
VIDU=N'📝 Đồng nghĩa: vui vẻ = hạnh phúc, nhanh = lẹ
📝 Trái nghĩa: sáng ↔ tối, khỏe ↔ yếu
📝 Đặt câu: "Bạn rất vui vẻ" = "Bạn rất hạnh phúc" ✓'
WHERE MABAIHOC=9;

UPDATE BAIHOC SET TENBAIHOC=N'Câu kể Ai là gì? Ai làm gì?',
NOIDUNGLYTUYET=N'Câu kể dùng để giới thiệu hoặc kể sự việc:
1. Ai là gì? → Giới thiệu (Lan là học sinh giỏi.)
2. Ai làm gì? → Kể hoạt động (Em đang đọc sách.)
3. Ai thế nào? → Tả đặc điểm (Bông hoa rất đẹp.)',
MEOGHINHO=N'💡 Xác định chủ ngữ (Ai?) rồi tìm vị ngữ (là gì/làm gì/thế nào).',
VIDU=N'📝 "Mẹ là bác sĩ." → Ai là gì? (Mẹ/là bác sĩ)
📝 "Em viết bài." → Ai làm gì? (Em/viết bài)
📝 "Trời rất đẹp." → Ai thế nào? (Trời/rất đẹp)'
WHERE MABAIHOC=10;

-- ========== TIẾNG ANH LỚP 3 ==========
UPDATE BAIHOC SET TENBAIHOC=N'My Family – Gia đình của em',
NOIDUNGLYTUYET=N'Các thành viên gia đình:
Father/Dad (bố), Mother/Mom (mẹ), Brother (anh/em trai), Sister (chị/em gái), Grandfather (ông), Grandmother (bà).

Cấu trúc: This is my + [thành viên].
Hỏi: Who is this? – Đây là ai?',
MEOGHINHO=N'💡 Grand = lớn → Grandfather = ông (bố lớn), Grandmother = bà.',
VIDU=N'📝 "This is my mother. She is a teacher."
📝 Q: "Who is this?" A: "This is my brother."
📝 "I love my family."'
WHERE MABAIHOC=11;

UPDATE BAIHOC SET TENBAIHOC=N'Animals – Các con vật',
NOIDUNGLYTUYET=N'Tên các con vật phổ biến:
Dog (chó), Cat (mèo), Bird (chim), Fish (cá), Rabbit (thỏ), Elephant (voi), Lion (sư tử), Monkey (khỉ).

Cấu trúc: I like + con vật. / It is a + con vật.
Hỏi: What animal is it?',
MEOGHINHO=N'💡 Số nhiều thêm "s": cats, dogs, birds. Riêng fish → fish (không đổi).',
VIDU=N'📝 "I like cats. They are cute."
📝 Q: "What animal is it?" A: "It is a rabbit."
📝 "I have two dogs and one fish."'
WHERE MABAIHOC=12;

-- ========== TOÁN LỚP 4 ==========
UPDATE BAIHOC SET TENBAIHOC=N'Phân số – Khái niệm và so sánh',
NOIDUNGLYTUYET=N'Phân số có tử số (trên) và mẫu số (dưới): a/b (b ≠ 0).
VD: 3/4 nghĩa là chia thành 4 phần bằng nhau, lấy 3 phần.

So sánh phân số cùng mẫu: tử lớn hơn → phân số lớn hơn.
So sánh phân số cùng tử: mẫu nhỏ hơn → phân số lớn hơn.',
MEOGHINHO=N'💡 Cùng mẫu: so tử. Cùng tử: mẫu nhỏ = phân số lớn.',
VIDU=N'📝 VD1: 3/5 và 2/5 → cùng mẫu → 3 > 2 → 3/5 > 2/5
📝 VD2: 1/3 và 1/5 → cùng tử → 3 < 5 → 1/3 > 1/5
📝 VD3: Pizza chia 8 phần, ăn 3 → ăn 3/8 pizza'
WHERE MABAIHOC=13;

UPDATE BAIHOC SET TENBAIHOC=N'Cộng trừ phân số cùng mẫu',
NOIDUNGLYTUYET=N'Cộng phân số cùng mẫu: giữ mẫu, cộng tử. a/c + b/c = (a+b)/c
Trừ phân số cùng mẫu: giữ mẫu, trừ tử. a/c - b/c = (a-b)/c

Nhớ rút gọn kết quả nếu được.',
MEOGHINHO=N'💡 Mẫu giữ nguyên, chỉ cộng/trừ tử số thôi!',
VIDU=N'📝 VD1: 2/7 + 3/7 = 5/7
📝 VD2: 5/6 - 1/6 = 4/6 = 2/3 (rút gọn chia 2)
📝 VD3: 1/4 + 2/4 = 3/4'
WHERE MABAIHOC=14;

-- ========== TIẾNG VIỆT LỚP 4 ==========
UPDATE BAIHOC SET TENBAIHOC=N'Danh từ, động từ, tính từ',
NOIDUNGLYTUYET=N'Ba từ loại quan trọng:
1. Danh từ: chỉ người, vật, hiện tượng (học sinh, cái bàn, mưa)
2. Động từ: chỉ hành động, trạng thái (chạy, nghĩ, yêu)
3. Tính từ: chỉ đặc điểm, tính chất (đỏ, thông minh, to lớn)',
MEOGHINHO=N'💡 Danh từ: đặt "một, những" trước. Động từ: đặt "đang, đã" trước. Tính từ: đặt "rất" trước.',
VIDU=N'📝 "Những bông hoa rất đẹp đang nở."
• bông hoa → danh từ (những bông hoa ✓)
• đẹp → tính từ (rất đẹp ✓)
• nở → động từ (đang nở ✓)'
WHERE MABAIHOC=15;

UPDATE BAIHOC SET TENBAIHOC=N'Viết đoạn văn tả người',
NOIDUNGLYTUYET=N'Cấu trúc đoạn văn tả người:
1. Giới thiệu: người đó là ai?
2. Tả ngoại hình: dáng người, khuôn mặt, mắt, mũi, miệng, tóc
3. Tả tính cách: hiền lành, vui vẻ, chăm chỉ...
4. Tình cảm: em yêu quý người đó',
MEOGHINHO=N'💡 Dùng từ gợi tả: đôi mắt sáng, nụ cười hiền, mái tóc đen nhánh.',
VIDU=N'📝 Đoạn văn mẫu:
"Mẹ em năm nay 35 tuổi. Mẹ có dáng người thon thả, mái tóc đen dài. Đôi mắt mẹ đen láy, luôn nhìn em trìu mến. Mẹ rất hiền lành và chăm chỉ. Em yêu mẹ rất nhiều."'
WHERE MABAIHOC=16;

-- ========== TIẾNG ANH LỚP 4 ==========
UPDATE BAIHOC SET TENBAIHOC=N'Daily Routines – Hoạt động hàng ngày',
NOIDUNGLYTUYET=N'Các hoạt động thường ngày:
Wake up (thức dậy), Brush teeth (đánh răng), Have breakfast (ăn sáng), Go to school (đi học), Have lunch (ăn trưa), Do homework (làm bài tập), Go to bed (đi ngủ).

Cấu trúc: I + động từ + thời gian.
"I wake up at 6 o''clock."',
MEOGHINHO=N'💡 at + giờ cụ thể: at 7 o''clock. in + buổi: in the morning.',
VIDU=N'📝 "I wake up at 6 o''clock. I brush my teeth and have breakfast. I go to school at 7 o''clock."
📝 Q: "What do you do in the morning?" A: "I have breakfast."'
WHERE MABAIHOC=17;

UPDATE BAIHOC SET TENBAIHOC=N'Food & Drinks – Đồ ăn và thức uống',
NOIDUNGLYTUYET=N'Đồ ăn: rice (cơm), bread (bánh mì), chicken (gà), fish (cá), noodles (mì), egg (trứng), fruit (trái cây).
Thức uống: water (nước), milk (sữa), juice (nước ép), tea (trà).

Cấu trúc: I like/don''t like + đồ ăn.
"Do you like...?" → "Yes, I do. / No, I don''t."',
MEOGHINHO=N'💡 Đếm được: an apple, two eggs. Không đếm: rice, water (dùng some).',
VIDU=N'📝 Q: "Do you like chicken?" A: "Yes, I do. I like chicken."
📝 "I have rice and fish for lunch."
📝 "Can I have some water, please?"'
WHERE MABAIHOC=18;

-- ========== TOÁN LỚP 5 ==========
UPDATE BAIHOC SET TENBAIHOC=N'Số thập phân và phép tính',
NOIDUNGLYTUYET=N'Số thập phân có phần nguyên và phần thập phân (ngăn bởi dấu phẩy).
VD: 3,25 → phần nguyên: 3, phần thập phân: 25

Cộng/Trừ: đặt tính thẳng dấu phẩy, tính như số tự nhiên.
Nhân: nhân bình thường, đếm chữ số thập phân rồi đặt dấu phẩy.',
MEOGHINHO=N'💡 Cộng/trừ: thẳng dấu phẩy. Nhân: đếm số chữ số sau dấu phẩy.',
VIDU=N'📝 VD1: 3,25 + 1,4 = 4,65 (thẳng dấu phẩy: 3,25 + 1,40)
📝 VD2: 5,8 - 2,3 = 3,5
📝 VD3: 1,2 × 3 = 3,6 (12×3=36, 1 chữ số thập phân → 3,6)'
WHERE MABAIHOC=19;

UPDATE BAIHOC SET TENBAIHOC=N'Diện tích và chu vi hình học',
NOIDUNGLYTUYET=N'Hình chữ nhật: Chu vi = (dài + rộng) × 2. Diện tích = dài × rộng.
Hình vuông: Chu vi = cạnh × 4. Diện tích = cạnh × cạnh.
Hình tam giác: Diện tích = đáy × chiều cao ÷ 2.',
MEOGHINHO=N'💡 Chu vi = đi quanh. Diện tích = bao phủ bên trong.',
VIDU=N'📝 HCN dài 8cm, rộng 5cm:
CV = (8+5)×2 = 26cm. DT = 8×5 = 40cm².

📝 Hình vuông cạnh 6cm:
CV = 6×4 = 24cm. DT = 6×6 = 36cm².

📝 Tam giác đáy 10cm, cao 4cm:
DT = 10×4÷2 = 20cm².'
WHERE MABAIHOC=20;

-- ========== TIẾNG VIỆT LỚP 5 ==========
UPDATE BAIHOC SET TENBAIHOC=N'Từ đồng âm và từ nhiều nghĩa',
NOIDUNGLYTUYET=N'Từ đồng âm: viết và đọc giống nhau nhưng nghĩa hoàn toàn khác nhau.
VD: "đường" (đường đi) và "đường" (đường ăn).

Từ nhiều nghĩa: một từ có nghĩa gốc và nghĩa chuyển (liên quan nhau).
VD: "chân" (chân người) → "chân bàn" (nghĩa chuyển).',
MEOGHINHO=N'💡 Đồng âm: nghĩa KHÁC HẲN. Nhiều nghĩa: nghĩa CÓ LIÊN QUAN.',
VIDU=N'📝 Đồng âm: "Bò đang ăn cỏ" (con bò) vs "Em bò trên sàn" (hành động bò)
📝 Nhiều nghĩa: "mắt người" (nghĩa gốc) → "mắt na" (nghĩa chuyển, có liên quan)'
WHERE MABAIHOC=21;

UPDATE BAIHOC SET TENBAIHOC=N'Liên kết câu trong đoạn văn',
NOIDUNGLYTUYET=N'Các cách liên kết câu:
1. Lặp từ ngữ: dùng lại từ ở câu trước
2. Dùng từ nối: vì vậy, tuy nhiên, nhưng, ngoài ra, hơn nữa
3. Thay thế từ ngữ: dùng đại từ (nó, họ, đó) thay cho danh từ đã nêu',
MEOGHINHO=N'💡 Đọc 2 câu liền, tìm từ lặp lại hoặc từ nối → đó là phép liên kết.',
VIDU=N'📝 Lặp từ: "Con mèo nằm trên ghế. Con mèo đang ngủ." → lặp "con mèo"
📝 Từ nối: "Trời mưa. Vì vậy, em không đi chơi." → nối bằng "vì vậy"
📝 Thay thế: "Lan rất chăm chỉ. Bạn ấy luôn đứng đầu lớp." → "bạn ấy" thay "Lan"'
WHERE MABAIHOC=22;

-- ========== TIẾNG ANH LỚP 5 ==========
UPDATE BAIHOC SET TENBAIHOC=N'Seasons & Weather – Mùa và thời tiết',
NOIDUNGLYTUYET=N'4 mùa: Spring (xuân), Summer (hè), Autumn/Fall (thu), Winter (đông).
Thời tiết: sunny (nắng), rainy (mưa), cloudy (mây), windy (gió), hot (nóng), cold (lạnh).

Hỏi: "What''s the weather like today?" → "It''s sunny."',
MEOGHINHO=N'💡 Thêm -y vào danh từ = tính từ: sun→sunny, rain→rainy, cloud→cloudy.',
VIDU=N'📝 Q: "What''s the weather like?" A: "It''s rainy and cold."
📝 "In summer, it is hot and sunny."
📝 "I like spring because it is warm."'
WHERE MABAIHOC=23;

UPDATE BAIHOC SET TENBAIHOC=N'My Hobbies – Sở thích của em',
NOIDUNGLYTUYET=N'Sở thích phổ biến: reading (đọc sách), swimming (bơi), singing (hát), dancing (nhảy), drawing (vẽ), playing football (đá bóng), cooking (nấu ăn).

Cấu trúc: My hobby is + V-ing. / I like + V-ing.
Hỏi: "What''s your hobby?" / "What do you like doing?"',
MEOGHINHO=N'💡 Sau "like" dùng V-ing: I like swimming (không phải swim).',
VIDU=N'📝 Q: "What''s your hobby?" A: "My hobby is reading books."
📝 "I like drawing pictures. It is fun!"
📝 "Do you like singing?" → "Yes, I do. I like singing very much."'
WHERE MABAIHOC=24;

PRINT N'Đã cập nhật xong 24 bài học!';
GO
