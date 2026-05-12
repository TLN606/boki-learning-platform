using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Boki_ELearning.Filters;
using Boki_ELearning.Models;
using Boki_ELearning.Models.ViewModels;

namespace Boki_ELearning.Controllers
{
    [AdminAuthorize]
    public class LessonAdminController : Controller
    {
        private readonly BokiDBEntities1 _db = new BokiDBEntities1();

        public ActionResult Index(string keyword = null, int? classId = null, int? subjectId = null)
        {
            var filter = new LessonAdminFilterViewModel
            {
                Keyword = keyword,
                ClassId = classId,
                SubjectId = subjectId
            };

            var lessons = _db.BAIHOCs.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .Select(x => new
                {
                    x.MABAIHOC,
                    x.TENBAIHOC,
                    x.NOIDUNGLYTUYET,
                    x.MEOGHINHO,
                    x.MALOP,
                    x.MAMONHOC,
                    ClassName = x.LOP.TENLOP,
                    SubjectName = x.MONHOC.TENMONHOC,
                    QuestionCount = x.CAUHOIs.Count(q => !q.IsDeleted)
                });

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var normalizedKeyword = keyword.Trim();
                lessons = lessons.Where(x => x.TENBAIHOC.Contains(normalizedKeyword));
            }

            if (classId.HasValue)
            {
                lessons = lessons.Where(x => x.MALOP == classId.Value);
            }

            if (subjectId.HasValue)
            {
                lessons = lessons.Where(x => x.MAMONHOC == subjectId.Value);
            }

            var model = new LessonAdminPageViewModel
            {
                Filter = filter,
                ClassOptions = LoadClassOptions(),
                SubjectOptions = LoadSubjectOptions(),
                Lessons = lessons
                    .OrderBy(x => x.ClassName)
                    .ThenBy(x => x.SubjectName)
                    .ThenBy(x => x.TENBAIHOC)
                    .ToList()
                    .Select(x => new LessonAdminListItemViewModel
                    {
                        LessonId = x.MABAIHOC,
                        LessonName = x.TENBAIHOC,
                        TheoryContent = x.NOIDUNGLYTUYET,
                        TipContent = x.MEOGHINHO,
                        ClassName = x.ClassName,
                        SubjectName = x.SubjectName,
                        QuestionCount = x.QuestionCount
                    })
                    .ToList()
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult Create()
        {
            var model = BuildEditor(new LessonAdminEditorViewModel());
            return View("Edit", model);
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            var lesson = _db.BAIHOCs.AsNoTracking()
                .Where(x => x.MABAIHOC == id && !x.IsDeleted)
                .Select(x => new LessonAdminEditorViewModel
                {
                    LessonId = x.MABAIHOC,
                    LessonName = x.TENBAIHOC,
                    ClassId = x.MALOP,
                    SubjectId = x.MAMONHOC,
                    TheoryContent = x.NOIDUNGLYTUYET,
                    TipContent = x.MEOGHINHO
                })
                .FirstOrDefault();

            if (lesson == null)
            {
                return HttpNotFound();
            }

            return View(BuildEditor(lesson));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(LessonAdminEditorViewModel model)
        {
            if (model == null)
            {
                TempData["Message"] = "Dữ liệu gửi lên không hợp lệ.";
                return RedirectToAction("Index");
            }

            ValidateEditor(model);

            var originalTheoryContent = model.TheoryContent ?? string.Empty;
            var originalTipContent = model.TipContent ?? string.Empty;

            if (ContainsBlockedHtmlPattern(originalTheoryContent))
            {
                ModelState.AddModelError("TheoryContent", "Nội dung bài học chứa thẻ script hoặc event nguy hiểm.");
            }

            if (ContainsBlockedHtmlPattern(originalTipContent))
            {
                ModelState.AddModelError("TipContent", "Mẹo ghi nhớ chứa thẻ script hoặc event nguy hiểm.");
            }

            model.TheoryContent = SanitizeRichHtml(originalTheoryContent);
            model.TipContent = SanitizeRichHtml(originalTipContent);

            if (string.IsNullOrWhiteSpace(StripHtml(model.TheoryContent)))
            {
                ModelState.AddModelError("TheoryContent", "Nội dung bài học không được để trống.");
            }

            if (!ModelState.IsValid)
            {
                return View("Edit", BuildEditor(model));
            }

            if (model.LessonId.HasValue)
            {
                var lesson = _db.BAIHOCs.FirstOrDefault(x => x.MABAIHOC == model.LessonId.Value && !x.IsDeleted);
                if (lesson == null)
                {
                    return HttpNotFound();
                }

                lesson.TENBAIHOC = model.LessonName.Trim();
                lesson.MALOP = model.ClassId;
                lesson.MAMONHOC = model.SubjectId;
                lesson.NOIDUNGLYTUYET = model.TheoryContent;
                lesson.MEOGHINHO = model.TipContent;

                TempData["Message"] = "Cập nhật bài học thành công.";
            }
            else
            {
                _db.BAIHOCs.Add(new BAIHOC
                {
                    TENBAIHOC = model.LessonName.Trim(),
                    MALOP = model.ClassId,
                    MAMONHOC = model.SubjectId,
                    NOIDUNGLYTUYET = model.TheoryContent,
                    MEOGHINHO = model.TipContent,
                    IsDeleted = false
                });

                TempData["Message"] = "Tạo bài học thành công.";
            }

            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var lesson = _db.BAIHOCs.FirstOrDefault(x => x.MABAIHOC == id && !x.IsDeleted);
            if (lesson == null)
            {
                return HttpNotFound();
            }

            lesson.IsDeleted = true;
            _db.SaveChanges();

            TempData["Message"] = "Đã ẩn bài học khỏi danh sách.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UploadMedia(HttpPostedFileBase upload)
        {
            if (upload == null || upload.ContentLength <= 0)
            {
                return BuildUploadError("Không tìm thấy file upload.");
            }

            const int maxSizeBytes = 25 * 1024 * 1024;
            if (upload.ContentLength > maxSizeBytes)
            {
                return BuildUploadError("Kích thước file vượt quá 25MB.");
            }

            var ext = Path.GetExtension(upload.FileName ?? string.Empty) ?? string.Empty;
            var imageExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" };
            var videoExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".mp4", ".webm", ".ogg", ".mov" };

            if (!imageExt.Contains(ext) && !videoExt.Contains(ext))
            {
                return BuildUploadError("Định dạng file không được hỗ trợ.");
            }

            var contentType = (upload.ContentType ?? string.Empty).Trim();
            var validContentType = contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)
                || contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase)
                || contentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase);
            if (!validContentType)
            {
                return BuildUploadError("Mime type không hợp lệ.");
            }

            var originalBaseName = Path.GetFileNameWithoutExtension(upload.FileName ?? string.Empty) ?? "media";
            var safeBaseName = Regex.Replace(originalBaseName, @"[^a-zA-Z0-9_-]", string.Empty);
            if (string.IsNullOrWhiteSpace(safeBaseName))
            {
                safeBaseName = "media";
            }

            var finalFileName = safeBaseName + "_" + DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "_" + Guid.NewGuid().ToString("N").Substring(0, 8) + ext.ToLowerInvariant();
            var uploadVirtualDir = "~/Content/uploads/lesson-media";
            var uploadPhysicalDir = Server.MapPath(uploadVirtualDir);
            if (uploadPhysicalDir == null)
            {
                return BuildUploadError("Không thể xác định thư mục lưu file.");
            }

            Directory.CreateDirectory(uploadPhysicalDir);
            upload.SaveAs(Path.Combine(uploadPhysicalDir, finalFileName));

            return Json(new
            {
                uploaded = 1,
                fileName = finalFileName,
                url = Url.Content(uploadVirtualDir + "/" + finalFileName)
            });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }

            base.Dispose(disposing);
        }

        private LessonAdminEditorViewModel BuildEditor(LessonAdminEditorViewModel model)
        {
            model.ClassOptions = LoadClassOptions();
            model.SubjectOptions = LoadSubjectOptions();
            return model;
        }

        private List<LessonAdminOptionViewModel> LoadClassOptions()
        {
            return _db.LOPs.AsNoTracking()
                .OrderBy(x => x.TENLOP)
                .Select(x => new LessonAdminOptionViewModel
                {
                    Id = x.MALOP,
                    Name = x.TENLOP
                })
                .ToList();
        }

        private List<LessonAdminOptionViewModel> LoadSubjectOptions()
        {
            return _db.MONHOCs.AsNoTracking()
                .OrderBy(x => x.TENMONHOC)
                .Select(x => new LessonAdminOptionViewModel
                {
                    Id = x.MAMONHOC,
                    Name = x.TENMONHOC
                })
                .ToList();
        }

        private void ValidateEditor(LessonAdminEditorViewModel model)
        {
            if (!_db.LOPs.AsNoTracking().Any(x => x.MALOP == model.ClassId))
            {
                ModelState.AddModelError("ClassId", "Lớp không tồn tại.");
            }

            if (!_db.MONHOCs.AsNoTracking().Any(x => x.MAMONHOC == model.SubjectId))
            {
                ModelState.AddModelError("SubjectId", "Môn học không tồn tại.");
            }
        }

        private ActionResult BuildUploadError(string message)
        {
            return Json(new
            {
                uploaded = 0,
                error = new { message = message ?? "Upload failed." }
            });
        }

        private static bool ContainsBlockedHtmlPattern(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return false;
            }

            return Regex.IsMatch(html, @"<\s*script\b", RegexOptions.IgnoreCase)
                || Regex.IsMatch(html, @"on\w+\s*=", RegexOptions.IgnoreCase)
                || Regex.IsMatch(html, @"javascript\s*:", RegexOptions.IgnoreCase);
        }

        private static string SanitizeRichHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            var sanitized = html.Trim();
            sanitized = Regex.Replace(sanitized, @"<\s*script\b[^>]*>.*?<\s*/\s*script\s*>", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            sanitized = Regex.Replace(sanitized, @"on\w+\s*=\s*(['""]).*?\1", string.Empty, RegexOptions.IgnoreCase | RegexOptions.Singleline);
            sanitized = Regex.Replace(sanitized, @"on\w+\s*=\s*[^\s>]+", string.Empty, RegexOptions.IgnoreCase);
            sanitized = Regex.Replace(sanitized, @"(href|src)\s*=\s*(['""])\s*javascript:[^'""]*\2", "$1=\"#\"", RegexOptions.IgnoreCase);

            return sanitized.Trim();
        }

        private static string StripHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return string.Empty;
            }

            var plain = Regex.Replace(html, "<.*?>", " ");
            plain = HttpUtility.HtmlDecode(plain);
            return string.IsNullOrWhiteSpace(plain) ? string.Empty : plain.Trim();
        }
    }
}
