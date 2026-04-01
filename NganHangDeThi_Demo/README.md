# Ngân Hàng Đề Thi (Demo tách riêng)

Thư mục này là bản demo web độc lập để tạo và quản lý câu hỏi trắc nghiệm, không can thiệp vào cấu trúc dự án `Boki_ELearning`.

## Vị trí

`D:\01_Workspaces\Coding\Websites\NganHangDeThi_Demo`

## Mở nhanh

1. Mở file `index.html` bằng trình duyệt.
2. Hoặc chạy local server tĩnh bất kỳ rồi truy cập vào thư mục này.

## Tính năng

- Tạo câu hỏi với: Môn học, Bài học, Độ khó, Trạng thái, Nội dung, 4 đáp án.
- Chọn đáp án đúng (1 đáp án).
- Sửa, xóa, đổi trạng thái câu hỏi.
- Tìm kiếm và lọc theo môn học, bài học, mức độ.
- Thống kê nhanh số lượng câu hỏi.
- Tạo đề ngẫu nhiên theo bộ lọc hiện tại.
- Xuất JSON và nhập JSON.
- Lưu dữ liệu vào `localStorage` của trình duyệt.

## Gợi ý tích hợp sau này

- Từ JSON xuất ra, có thể viết API nhận dữ liệu để map vào các bảng:
  - `CAUHOI.NOIDUNGCAUHOI`
  - `CAUHOI.MABAIHOC`
  - `DAPAN.NOIDUNGDAPAN`
  - `DAPAN.DUNGLSAI`
