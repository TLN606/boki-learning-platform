# Boki Learning Platform 🎓

*Lưu ý: Bạn có thể xem bản [Tiếng Anh tại đây (English Version)](./README.md).*

Boki Learning Platform là một nền tảng giáo dục hiện đại, tương tác cao và thú vị được thiết kế để mang lại trải nghiệm học tập vui vẻ. Sở hữu giao diện **Claymorphism** sáng tạo, các hiệu ứng chuyển động mượt mà và chế độ tối (dark mode) được tích hợp hoàn hảo, Boki đảm bảo một trải nghiệm trực quan hóa tối ưu cho cả học sinh và quản trị viên.

## ✨ Các tính năng nổi bật

### 👨‍🎓 Dành cho Học sinh (Student)
- **Bảng điều khiển tương tác (Dashboard)**: Theo dõi tiến độ học tập, bài học sắp tới và thành tích.
- **Bài giảng sinh động**: Giao diện bài giảng tối ưu hóa để tăng khả năng tập trung và dễ đọc.
- **Luyện thi trắc nghiệm (Quizzes)**: Giao diện làm bài trắc nghiệm trực quan với hệ thống phản hồi tức thì.
- **Theo dõi kết quả**: Tóm tắt chi tiết kết quả học tập và đánh giá năng lực.

### 👨‍🏫 Dành cho Quản trị viên (Admin)
- **Bảng phân tích (Analytics Dashboard)**: Tổng quan về năng lực học tập của học sinh và mức độ tương tác trên nền tảng.
- **Quản lý Bài giảng (Lesson Management)**: Tạo, chỉnh sửa và sắp xếp nội dung học tập.
- **Ngân hàng Câu hỏi (Question Bank)**: Quản lý câu hỏi, đáp án và cấp độ khó. (kèm tính năng thuật toán chia câu hỏi tự động).
- **Theo dõi Kết quả**: Giám sát điểm số, lịch sử làm bài thi thử và thống kê chi tiết.
- **Quản lý Học viên**: Quản lý tài khoản hệ thống của người học.

## 🎨 Điểm nhấn UI/UX
- **Hệ thống Thiết kế Claymorphism**: Sử dụng các khối thẻ dạng nổi khối mềm mại, tạo phong cách học thuật hiện đại pha yếu tố vui nhộn với màu sắc bắt mắt.
- **Hỗ trợ Dark Mode**: Chuyển đổi mượt mà giữa giao diện sáng/tối trên hầu hết hệ thống trang.
- **Thiết kế Đáp ứng (Responsive Setup)**: Tối ưu hoá hiển thị đáp ứng màn hình từ di động (Mobile) tới máy tính để bàn (Desktop).
- **Micro-Animations**: Các hiệu ứng hover di chuột và chuyển động tương tác mượt mà làm tăng trải nghiệm dùng.

## 🚀 Hướng dẫn cài đặt

### 1. Clone mã nguồn
```bash
git clone https://github.com/TLN606/boki-learning-platform.git
```

### 2. Xem bản Demo thiết kế giao diện UI V1.0 nguyên thủy
```bash
git checkout demo-v1.0
```

### 3. Trải nghiệm Demo Giao diện tĩnh (Frontend UI V1.0)
Truy cập vào thư mục `ui-premium-redesign-v1/` và mở file `index.html` lên trình duyệt web. Khuyến khích sử dụng chế độ Live Server.

### 4. Khởi chạy dự án nguyên cấu trúc Backend
*Mở file `Boki_ELearning.slnx` hoặc dùng *.csproj với nền tảng phần mềm Visual Studio để bắt đầu khởi chạy cấu trúc back-end bằng ASP.NET C#.**

## 🛠️ Công nghệ sử dụng
- **Giao diện Frontend**: Cấu trúc HTML5 chuẩn SEO, CSS3 Native, Vanilla JS, sử dụng bộ Icons dạng SVG nhằm giảm băng thông.
- **Framework Backend**: C# phối hợp mô hình ASP.NET MVC.

## 📄 Cấu trúc Quản trị Nhánh (Branching)
- **main**: Nhánh phát triển chính (cập nhật mới nhất của các code logic tính năng hàng ngày).
- **demo-v1.0**: Lưu trữ cố định cho trọn bộ UI Mockup của bản Demo tính năng thiết kế 1.0.
