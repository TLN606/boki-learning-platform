# CONTEXT HANDOFF

## Done
- Tạo và áp dụng migration DB: `App_Data\Sql\20260409_BokiV1_ExamModule.sql`.
- Tạo và áp dụng script reset admin dev: `App_Data\Sql\20260409_Reset_Admin.sql`.
- Thêm metadata ngân hàng câu hỏi: `CAUHOI.MUCDO`, `CAUHOI.TRANGTHAI`.
- Tạo đầy đủ bảng đề thi tổng hợp snapshot:
  - `DETHI`, `DETHI_MONHOC`, `DETHI_LOP`, `DETHI_HOCSINH`, `DETHI_CAUHOI`, `DETHI_LANLAM`, `DETHI_LANLAM_CHITIET`.
- Cập nhật auth/role:
  - `Helpers\RoleHelper.cs`, `Filters\AdminAuthorizeAttribute.cs`, chuẩn hóa role trong `AccountController`.
- Hoàn thiện luồng học sinh:
  - `LearningController` có `Lesson`, `Quiz`, `SubmitQuiz`, `Result`.
  - Lưu điểm thang 10 + chi tiết trả lời vào `KETQUA` / `CHITIKETQUA`.
- Triển khai trang quản lý:
  - `AdminController` + views `Dashboard/Students/Results`.
- Triển khai ngân hàng câu hỏi:
  - `QuestionBankController` + view `Views\QuestionBank\Index.cshtml`.
  - CRUD, soft delete, filter, import JSON, export JSON.
- Triển khai đề thi tổng hợp:
  - `ExamAdminController` + views `Views\ExamAdmin\Index/Create/Detail`.
  - Tạo đề random snapshot theo lớp + môn, publish/close đề.
- Triển khai phía học sinh cho đề tổng hợp:
  - `StudentExamController` + views `Views\StudentExam\Index/Take/History`.
  - Nhiều lần làm, lưu toàn bộ lịch sử, hiển thị điểm cao nhất.
- Chuẩn hóa UI/layout:
  - `Views\Layouts\_UserLayout.cshtml`, `Views\Layouts\_AdminLayout.cshtml`, `Content\Site.css`.
- Chuẩn hóa UTF-8 cho source/view/sql đang dùng, quét marker mojibake không phát hiện lỗi.
- Đồng bộ `Boki_ELearning.csproj` để include file mới.

## In Progress
- Không có hạng mục đang làm dở.

## Pending
- Chưa chạy được build chính thức bằng MSBuild của Visual Studio do môi trường thiếu `Microsoft.WebApplication.targets`.
- Chưa chạy test end-to-end bằng trình duyệt (manual clickflow).
- Cần dọn untracked `../.dotnet/` nếu muốn workspace sạch hoàn toàn.

## Runbook
- Áp migration DB:
  - `20260409_BokiV1_ExamModule.sql` => thành công.
- Áp script reset admin:
  - `20260409_Reset_Admin.sql` => thành công.
- Smoke query DB:
  - Kiểm tra cột mới `MUCDO/TRANGTHAI` => tồn tại, dữ liệu không null.
  - Kiểm tra bảng `DETHI*` => tồn tại đầy đủ.
  - Kiểm tra admin `admin@boki.edu.vn` => role `Admin`, hash `PBKDF2`.
- Kiểm tra syntax C# bằng Roslyn compiler cục bộ:
  - `bin\roslyn\csc.exe` compile all `.cs` với reference thủ công => `ExitCode=0`.
- Quét mojibake trên source/view/sql đang dùng:
  - Không thấy marker `Ã`, `Â`, `�`.
