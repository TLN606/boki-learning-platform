using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using Boki_ELearning.Filters;
using Boki_ELearning.Helpers;
using Boki_ELearning.Models;
using Boki_ELearning.Models.ViewModels;

namespace Boki_ELearning.Controllers
{
    [AdminAuthorize]
    public class ExamAdminController : Controller
    {
        private readonly BokiDBEntities1 _db = new BokiDBEntities1();

        public ActionResult Index()
        {
            var model = new List<AdminExamListItemViewModel>();

            using (var conn = DbConnectionHelper.CreateSqlConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"SELECT E.MADETHI,
                             E.TENDETHI,
                             E.SOCAU,
                             E.THOILUONGPHUT,
                             E.TRANGTHAI,
                             E.NGAYTAO,
                             (SELECT COUNT(1) FROM DETHI_HOCSINH DH WHERE DH.MADETHI = E.MADETHI) AS AssignedCount
                      FROM DETHI E
                      ORDER BY E.MADETHI DESC", conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        model.Add(new AdminExamListItemViewModel
                        {
                            ExamId = reader.GetInt32(0),
                            ExamName = reader.GetString(1),
                            QuestionCount = reader.GetInt32(2),
                            DurationMinutes = reader.GetInt32(3),
                            Status = reader.GetString(4),
                            CreatedAt = reader.GetDateTime(5),
                            AssignedStudentCount = reader.GetInt32(6)
                        });
                    }
                }
            }

            return View(model);
        }

        public ActionResult Create()
        {
            var form = new AdminExamCreateViewModel
            {
                QuestionCount = 10,
                DurationMinutes = 30,
                SubjectIds = new List<int>(),
                ClassIds = new List<int>()
            };

            var model = BuildCreatePage(form);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AdminExamCreateViewModel form)
        {
            form.SubjectIds = (form.SubjectIds ?? new List<int>()).Where(x => x > 0).Distinct().ToList();
            form.ClassIds = (form.ClassIds ?? new List<int>()).Where(x => x > 0).Distinct().ToList();
            var availableCount = CountAvailableQuestions(form.SubjectIds, form.ClassIds);

            if (!form.SubjectIds.Any())
            {
                ModelState.AddModelError("SubjectIds", "Vui lòng chọn ít nhất một môn học.");
            }

            if (!form.ClassIds.Any())
            {
                ModelState.AddModelError("ClassIds", "Vui lòng chọn ít nhất một lớp.");
            }

            if (!ModelState.IsValid)
            {
                return View(BuildCreatePage(form, availableCount));
            }

            if (form.QuestionCount > availableCount)
            {
                ModelState.AddModelError("QuestionCount", $"Không đủ câu hỏi Active để tạo đề. Hiện có {availableCount}, cần {form.QuestionCount}.");
                return View(BuildCreatePage(form, availableCount));
            }

            var accountId = Session[SessionKeys.AccountId] as int?;
            if (!accountId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var subjectCsv = string.Join(",", form.SubjectIds);
            var classCsv = string.Join(",", form.ClassIds);

            int examId;
            using (var conn = DbConnectionHelper.CreateSqlConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    var picked = new List<int>();
                    using (var poolCmd = new SqlCommand(
                        @"SELECT TOP (@Take) C.MACAUHOI
                          FROM CAUHOI C
                          INNER JOIN BAIHOC B ON B.MABAIHOC = C.MABAIHOC
                          WHERE C.IsDeleted = 0
                            AND C.TRANGTHAI = N'Active'
                            AND B.IsDeleted = 0
                            AND B.MAMONHOC IN (SELECT TRY_CAST([value] AS INT) FROM STRING_SPLIT(@SubjectCsv, ','))
                            AND B.MALOP IN (SELECT TRY_CAST([value] AS INT) FROM STRING_SPLIT(@ClassCsv, ','))
                          ORDER BY NEWID()", conn, tx))
                    {
                        poolCmd.Parameters.AddWithValue("@Take", form.QuestionCount);
                        poolCmd.Parameters.AddWithValue("@SubjectCsv", subjectCsv);
                        poolCmd.Parameters.AddWithValue("@ClassCsv", classCsv);
                        using (var reader = poolCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                picked.Add(reader.GetInt32(0));
                            }
                        }
                    }

                    if (picked.Count < form.QuestionCount)
                    {
                        tx.Rollback();
                        ModelState.AddModelError("QuestionCount", $"Không đủ câu hỏi Active để tạo đề. Hiện có {picked.Count}, cần {form.QuestionCount}.");
                        return View(BuildCreatePage(form, picked.Count));
                    }

                    using (var createCmd = new SqlCommand(
                        @"INSERT INTO DETHI (TENDETHI, THOILUONGPHUT, SOCAU, TRANGTHAI, NGAYMO, NGAYDONG, MATAIKHOAN_TAO)
                          VALUES (@Name, @Duration, @Count, N'Draft', @OpenAt, @CloseAt, @AccountId);
                          SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tx))
                    {
                        createCmd.Parameters.AddWithValue("@Name", form.ExamName.Trim());
                        createCmd.Parameters.AddWithValue("@Duration", form.DurationMinutes);
                        createCmd.Parameters.AddWithValue("@Count", form.QuestionCount);
                        createCmd.Parameters.AddWithValue("@OpenAt", (object)form.OpenAt ?? DBNull.Value);
                        createCmd.Parameters.AddWithValue("@CloseAt", (object)form.CloseAt ?? DBNull.Value);
                        createCmd.Parameters.AddWithValue("@AccountId", accountId.Value);
                        examId = Convert.ToInt32(createCmd.ExecuteScalar());
                    }

                    foreach (var subjectId in form.SubjectIds)
                    {
                        using (var cmd = new SqlCommand(
                            "INSERT INTO DETHI_MONHOC (MADETHI, MAMONHOC) VALUES (@ExamId, @SubjectId)", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@ExamId", examId);
                            cmd.Parameters.AddWithValue("@SubjectId", subjectId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    foreach (var classId in form.ClassIds)
                    {
                        using (var cmd = new SqlCommand(
                            "INSERT INTO DETHI_LOP (MADETHI, MALOP) VALUES (@ExamId, @ClassId)", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@ExamId", examId);
                            cmd.Parameters.AddWithValue("@ClassId", classId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    using (var assignCmd = new SqlCommand(
                        @"INSERT INTO DETHI_HOCSINH (MADETHI, MAHOCSINH)
                          SELECT @ExamId, HS.MAHOCSINH
                          FROM HOCSINH HS
                          WHERE HS.MALOP IN (SELECT TRY_CAST([value] AS INT) FROM STRING_SPLIT(@ClassCsv, ','))", conn, tx))
                    {
                        assignCmd.Parameters.AddWithValue("@ExamId", examId);
                        assignCmd.Parameters.AddWithValue("@ClassCsv", classCsv);
                        assignCmd.ExecuteNonQuery();
                    }

                    for (var i = 0; i < picked.Count; i++)
                    {
                        using (var cmd = new SqlCommand(
                            "INSERT INTO DETHI_CAUHOI (MADETHI, MACAUHOI, STT) VALUES (@ExamId, @QuestionId, @Order)", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@ExamId", examId);
                            cmd.Parameters.AddWithValue("@QuestionId", picked[i]);
                            cmd.Parameters.AddWithValue("@Order", i + 1);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                }
            }

            TempData["Message"] = "Tạo đề thi thành công. Bạn có thể bấm Publish để học sinh bắt đầu làm.";
            return RedirectToAction("Detail", new { id = examId });
        }

        [HttpGet]
        public JsonResult AvailableQuestionCount(string subjectIds = null, string classIds = null)
        {
            var parsedSubjects = ParseCsvIds(subjectIds);
            var parsedClasses = ParseCsvIds(classIds);
            var count = CountAvailableQuestions(parsedSubjects, parsedClasses);
            return Json(new { count }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Detail(int id)
        {
            AdminExamDetailViewModel model;
            using (var conn = DbConnectionHelper.CreateSqlConnection())
            {
                conn.Open();
                using (var examCmd = new SqlCommand(
                    @"SELECT E.MADETHI,
                             E.TENDETHI,
                             E.SOCAU,
                             E.THOILUONGPHUT,
                             E.TRANGTHAI,
                             E.NGAYTAO,
                             E.NGAYMO,
                             E.NGAYDONG
                      FROM DETHI E
                      WHERE E.MADETHI = @ExamId", conn))
                {
                    examCmd.Parameters.AddWithValue("@ExamId", id);
                    using (var reader = examCmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return HttpNotFound();
                        }

                        model = new AdminExamDetailViewModel
                        {
                            ExamId = reader.GetInt32(0),
                            ExamName = reader.GetString(1),
                            QuestionCount = reader.GetInt32(2),
                            DurationMinutes = reader.GetInt32(3),
                            Status = reader.GetString(4),
                            CreatedAt = reader.GetDateTime(5),
                            OpenAt = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6),
                            CloseAt = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                            Subjects = new List<string>(),
                            Classes = new List<string>(),
                            StudentResults = new List<AdminExamStudentResultViewModel>()
                        };
                    }
                }

                using (var subjectCmd = new SqlCommand(
                    @"SELECT M.TENMONHOC
                      FROM DETHI_MONHOC DM
                      INNER JOIN MONHOC M ON M.MAMONHOC = DM.MAMONHOC
                      WHERE DM.MADETHI = @ExamId
                      ORDER BY M.TENMONHOC", conn))
                {
                    subjectCmd.Parameters.AddWithValue("@ExamId", id);
                    using (var reader = subjectCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.Subjects.Add(reader.GetString(0));
                        }
                    }
                }

                using (var classCmd = new SqlCommand(
                    @"SELECT L.TENLOP
                      FROM DETHI_LOP DL
                      INNER JOIN LOP L ON L.MALOP = DL.MALOP
                      WHERE DL.MADETHI = @ExamId
                      ORDER BY L.TENLOP", conn))
                {
                    classCmd.Parameters.AddWithValue("@ExamId", id);
                    using (var reader = classCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.Classes.Add(reader.GetString(0));
                        }
                    }
                }

                using (var resultCmd = new SqlCommand(
                    @"SELECT HS.MAHOCSINH,
                             HS.HOTEN,
                             L.TENLOP,
                             ISNULL(A.AttemptCount, 0),
                             A.BestScore,
                             A.LatestAttemptAt
                      FROM DETHI_HOCSINH DH
                      INNER JOIN HOCSINH HS ON HS.MAHOCSINH = DH.MAHOCSINH
                      INNER JOIN LOP L ON L.MALOP = HS.MALOP
                      LEFT JOIN
                      (
                          SELECT MAHOCSINH,
                                 COUNT(1) AS AttemptCount,
                                 MAX(DIEMSO) AS BestScore,
                                 MAX(NGAYNOP) AS LatestAttemptAt
                          FROM DETHI_LANLAM
                          WHERE MADETHI = @ExamId
                          GROUP BY MAHOCSINH
                      ) A ON A.MAHOCSINH = HS.MAHOCSINH
                      WHERE DH.MADETHI = @ExamId
                      ORDER BY L.TENLOP, HS.HOTEN", conn))
                {
                    resultCmd.Parameters.AddWithValue("@ExamId", id);
                    using (var reader = resultCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.StudentResults.Add(new AdminExamStudentResultViewModel
                            {
                                StudentId = reader.GetInt32(0),
                                StudentName = reader.GetString(1),
                                ClassName = reader.GetString(2),
                                AttemptCount = reader.GetInt32(3),
                                BestScore = reader.IsDBNull(4) ? (double?)null : reader.GetDouble(4),
                                LatestAttemptAt = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5)
                            });
                        }
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Publish(int id)
        {
            using (var conn = DbConnectionHelper.CreateSqlConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"UPDATE DETHI
                      SET TRANGTHAI = N'Published',
                          NGAYMO = ISNULL(NGAYMO, GETDATE()),
                          NGAYSUA = GETDATE()
                      WHERE MADETHI = @ExamId", conn))
                {
                    cmd.Parameters.AddWithValue("@ExamId", id);
                    cmd.ExecuteNonQuery();
                }
            }

            TempData["Message"] = "Đề thi đã được Publish.";
            return RedirectToAction("Detail", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Close(int id)
        {
            using (var conn = DbConnectionHelper.CreateSqlConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"UPDATE DETHI
                      SET TRANGTHAI = N'Closed',
                          NGAYDONG = ISNULL(NGAYDONG, GETDATE()),
                          NGAYSUA = GETDATE()
                      WHERE MADETHI = @ExamId", conn))
                {
                    cmd.Parameters.AddWithValue("@ExamId", id);
                    cmd.ExecuteNonQuery();
                }
            }

            TempData["Message"] = "Đề thi đã được đóng.";
            return RedirectToAction("Detail", new { id });
        }

        private AdminExamCreatePageViewModel BuildCreatePage(AdminExamCreateViewModel form, int availableCount = 0)
        {
            form = form ?? new AdminExamCreateViewModel();
            form.SubjectIds = form.SubjectIds ?? new List<int>();
            form.ClassIds = form.ClassIds ?? new List<int>();

            return new AdminExamCreatePageViewModel
            {
                Form = form,
                SubjectOptions = LoadSubjectOptions(),
                ClassOptions = LoadClassOptions(),
                AvailableQuestionCount = availableCount
            };
        }

        private int CountAvailableQuestions(List<int> subjectIds, List<int> classIds)
        {
            subjectIds = (subjectIds ?? new List<int>()).Where(x => x > 0).Distinct().ToList();
            classIds = (classIds ?? new List<int>()).Where(x => x > 0).Distinct().ToList();
            if (!subjectIds.Any() || !classIds.Any())
            {
                return 0;
            }

            using (var conn = DbConnectionHelper.CreateSqlConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"SELECT COUNT(1)
                      FROM CAUHOI C
                      INNER JOIN BAIHOC B ON B.MABAIHOC = C.MABAIHOC
                      WHERE C.IsDeleted = 0
                        AND C.TRANGTHAI = N'Active'
                        AND B.IsDeleted = 0
                        AND B.MAMONHOC IN (SELECT TRY_CAST([value] AS INT) FROM STRING_SPLIT(@SubjectCsv, ','))
                        AND B.MALOP IN (SELECT TRY_CAST([value] AS INT) FROM STRING_SPLIT(@ClassCsv, ','))", conn))
                {
                    cmd.Parameters.AddWithValue("@SubjectCsv", string.Join(",", subjectIds));
                    cmd.Parameters.AddWithValue("@ClassCsv", string.Join(",", classIds));
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        private static List<int> ParseCsvIds(string csv)
        {
            if (string.IsNullOrWhiteSpace(csv))
            {
                return new List<int>();
            }

            return csv
                .Split(',')
                .Select(x =>
                {
                    int value;
                    return int.TryParse(x, out value) ? value : 0;
                })
                .Where(x => x > 0)
                .Distinct()
                .ToList();
        }

        private List<SimpleOptionViewModel> LoadSubjectOptions()
        {
            return _db.MONHOCs.AsNoTracking()
                .OrderBy(x => x.TENMONHOC)
                .Select(x => new SimpleOptionViewModel
                {
                    Id = x.MAMONHOC,
                    Name = x.TENMONHOC
                })
                .ToList();
        }

        private List<SimpleOptionViewModel> LoadClassOptions()
        {
            return _db.LOPs.AsNoTracking()
                .OrderBy(x => x.TENLOP)
                .Select(x => new SimpleOptionViewModel
                {
                    Id = x.MALOP,
                    Name = x.TENLOP
                })
                .ToList();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
