# Boki Learning Platform 🎓

Nền tảng học tập trực tuyến cho học sinh tiểu học, xây dựng bằng ASP.NET MVC. Học sinh học bài, làm quiz và thi đề tổng hợp; quản trị viên quản lý bài giảng, ngân hàng câu hỏi, đề thi và theo dõi kết quả.

## Tính năng

**Học sinh** (role `HocSinh`)
- Xem danh sách bài học theo lớp và môn, đọc bài giảng, làm quiz sau bài.
- Điểm thang 10, lưu chi tiết từng câu trả lời, xem lại kết quả.
- Làm đề thi tổng hợp do admin phát hành: được làm nhiều lần, lưu toàn bộ lịch sử và hiển thị điểm cao nhất.

**Quản trị viên** (role `Admin`)
- Dashboard thống kê, quản lý học sinh, theo dõi kết quả.
- Quản lý bài giảng (`LessonAdmin`).
- Ngân hàng câu hỏi (`QuestionBank`): thêm/sửa/xoá mềm, lọc theo mức độ và trạng thái, import/export JSON.
- Đề thi tổng hợp (`ExamAdmin`): tạo đề ngẫu nhiên theo lớp + môn (lưu snapshot câu hỏi), phát hành và đóng đề.

## Công nghệ

| Thành phần | Chi tiết |
|---|---|
| Backend | ASP.NET MVC 5.2.9, .NET Framework 4.7.2, C# |
| Dữ liệu | SQL Server, Entity Framework 5.0 (Database First, `BokiModel.edmx`) |
| Giao diện | Razor, Bootstrap 5.2.3, jQuery 3.7.0, CSS giao diện Claymorphism + dark mode (`Content/templates/boki-frontend`) |
| Bảo mật | Mật khẩu băm PBKDF2, phân quyền bằng session + `AdminAuthorize` / `SessionAuthorize` |

## Cài đặt và chạy

**Yêu cầu:** Visual Studio 2022 bản mới (mở được file `.slnx`) có workload ASP.NET, và SQL Server.

1. Clone repo:
   ```bash
   git clone https://github.com/TLN606/boki-learning-platform.git
   ```
2. Tạo cơ sở dữ liệu `BokiDB`, chọn một trong hai cách:
   - Chạy `Boki_ELearning/App_Data/Sql/Backups/script.sql` (chỉ có cấu trúc, không có dữ liệu). Script đang trỏ đường dẫn file `.mdf` trên máy tác giả, hãy sửa `FILENAME` cho phù hợp trước khi chạy.
   - Hoặc restore `Boki_ELearning/App_Data/Sql/Backups/BokiDB_20260409_051744.bak` bằng SSMS.
3. Kiểm tra chuỗi kết nối `BokiDBEntities1` trong `Boki_ELearning/Web.config`. Mặc định: `data source=.`, `initial catalog=BokiDB`, `integrated security=True`.
4. Mở `Boki_ELearning/Boki_ELearning.slnx` (hoặc `.csproj`) bằng Visual Studio, restore NuGet rồi nhấn **F5**.

Route mặc định: `{controller}/{action}/{id}`, trang đầu là `Home/Index`.

## Sao lưu cơ sở dữ liệu

```powershell
powershell -ExecutionPolicy Bypass -File .\Boki_ELearning\Scripts\backup-bokidb.ps1
```

Xuất `.bacpac` thêm `-Format bacpac` (cần `SqlPackage`). Xem thêm `Boki_ELearning/Scripts/README_BACKUP.md`.

## Cấu trúc thư mục

```
Boki_ELearning/
├── Controllers/     Account, Home, Learning, StudentExam, Admin, LessonAdmin, QuestionBank, ExamAdmin
├── Models/          BokiModel.edmx + entity + ViewModels
├── Views/           Razor views theo từng controller, Layouts/_UserLayout, _AdminLayout
├── Filters/         AdminAuthorize, SessionAuthorize
├── Helpers/         PasswordHelper, RoleHelper, SessionKeys, DbConnectionHelper
├── Content/         Bootstrap, Site.css, templates/boki-frontend (giao diện tĩnh gốc)
├── Scripts/         jQuery, Bootstrap, backup-bokidb.ps1
└── App_Data/Sql/    script.sql và bản backup .bak
```

## Nhánh

- `main`: nhánh chính.
- `archive/*`: bản lưu trữ nguyên trạng các nhánh cũ, không phát triển thêm.
