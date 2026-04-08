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
    [SessionAuthorize]
    public class StudentExamController : Controller
    {
        private readonly BokiDBEntities1 _db = new BokiDBEntities1();

        public ActionResult Index()
        {
            var studentId = Session[SessionKeys.StudentId] as int?;
            if (!studentId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new List<StudentExamListItemViewModel>();
            using (var conn = DbConnectionHelper.CreateSqlConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"SELECT E.MADETHI,
                             E.TENDETHI,
                             E.SOCAU,
                             E.THOILUONGPHUT,
                             E.TRANGTHAI,
                             CASE WHEN E.TENDETHI LIKE N'[[]Tự luyện]%' THEN 1 ELSE 0 END AS IsPractice,
                             A.BestScore,
                             ISNULL(A.AttemptCount, 0) AS AttemptCount,
                             A.LatestAttemptAt
                      FROM DETHI_HOCSINH DH
                      INNER JOIN DETHI E ON E.MADETHI = DH.MADETHI
                      LEFT JOIN
                      (
                          SELECT MADETHI,
                                 COUNT(1) AS AttemptCount,
                                 MAX(DIEMSO) AS BestScore,
                                 MAX(NGAYNOP) AS LatestAttemptAt
                          FROM DETHI_LANLAM
                          WHERE MAHOCSINH = @StudentId
                          GROUP BY MADETHI
                      ) A ON A.MADETHI = E.MADETHI
                      WHERE DH.MAHOCSINH = @StudentId
                      ORDER BY E.MADETHI DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@StudentId", studentId.Value);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.Add(new StudentExamListItemViewModel
                            {
                                ExamId = reader.GetInt32(0),
                                ExamName = reader.GetString(1),
                                QuestionCount = reader.GetInt32(2),
                                DurationMinutes = reader.GetInt32(3),
                                Status = reader.GetString(4),
                                IsPractice = reader.GetInt32(5) == 1,
                                BestScore = reader.IsDBNull(6) ? (double?)null : reader.GetDouble(6),
                                AttemptCount = reader.GetInt32(7),
                                LatestAttemptAt = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8)
                            });
                        }
                    }
                }
            }

            return View(model);
        }

        public ActionResult CreatePractice()
        {
            var studentId = Session[SessionKeys.StudentId] as int?;
            if (!studentId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new StudentPracticeCreatePageViewModel
            {
                Form = new StudentPracticeCreateViewModel
                {
                    ClassId = Session[SessionKeys.ClassId] as int?,
                    QuestionCount = 10,
                    DurationMinutes = 20
                },
                ClassOptions = LoadClassOptions(),
                SubjectOptions = LoadSubjectOptions(),
                AvailableQuestionCount = 0
            };

            if (model.Form.ClassId.HasValue && model.SubjectOptions.Any())
            {
                model.Form.SubjectId = model.SubjectOptions.First().Id;
                model.AvailableQuestionCount = CountAvailableQuestions(model.Form.SubjectId.Value, model.Form.ClassId.Value);
                if (model.AvailableQuestionCount > 0)
                {
                    model.Form.QuestionCount = Math.Min(model.Form.QuestionCount, model.AvailableQuestionCount);
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreatePractice(StudentPracticeCreateViewModel form)
        {
            var studentId = Session[SessionKeys.StudentId] as int?;
            var accountId = Session[SessionKeys.AccountId] as int?;
            if (!studentId.HasValue || !accountId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var classId = form.ClassId.GetValueOrDefault();
            var subjectId = form.SubjectId.GetValueOrDefault();
            var availableCount = (form.ClassId.HasValue && form.SubjectId.HasValue)
                ? CountAvailableQuestions(subjectId, classId)
                : 0;

            if (!ModelState.IsValid)
            {
                return View(BuildPracticePage(form, availableCount));
            }

            if (form.QuestionCount > availableCount)
            {
                ModelState.AddModelError("QuestionCount", $"Không đủ câu hỏi để tạo đề. Hiện có {availableCount} câu khả dụng.");
                return View(BuildPracticePage(form, availableCount));
            }

            var pickedQuestionIds = new List<int>();
            int examId;
            using (var conn = DbConnectionHelper.CreateSqlConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    using (var pickCmd = new SqlCommand(
                        @"SELECT TOP (@Take) C.MACAUHOI
                          FROM CAUHOI C
                          INNER JOIN BAIHOC B ON B.MABAIHOC = C.MABAIHOC
                          WHERE C.IsDeleted = 0
                            AND C.TRANGTHAI = N'Active'
                            AND B.IsDeleted = 0
                            AND B.MAMONHOC = @SubjectId
                            AND B.MALOP = @ClassId
                          ORDER BY NEWID()", conn, tx))
                    {
                        pickCmd.Parameters.AddWithValue("@Take", form.QuestionCount);
                        pickCmd.Parameters.AddWithValue("@SubjectId", subjectId);
                        pickCmd.Parameters.AddWithValue("@ClassId", classId);

                        using (var reader = pickCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                pickedQuestionIds.Add(reader.GetInt32(0));
                            }
                        }
                    }

                    if (pickedQuestionIds.Count < form.QuestionCount)
                    {
                        tx.Rollback();
                        ModelState.AddModelError("QuestionCount", $"Không đủ câu hỏi để tạo đề. Hiện có {pickedQuestionIds.Count} câu khả dụng.");
                        return View(BuildPracticePage(form, pickedQuestionIds.Count));
                    }

                    var className = _db.LOPs.AsNoTracking()
                        .Where(x => x.MALOP == classId)
                        .Select(x => x.TENLOP)
                        .FirstOrDefault() ?? "Lớp";
                    var subjectName = _db.MONHOCs.AsNoTracking()
                        .Where(x => x.MAMONHOC == subjectId)
                        .Select(x => x.TENMONHOC)
                        .FirstOrDefault() ?? "Môn";

                    using (var createCmd = new SqlCommand(
                        @"INSERT INTO DETHI (TENDETHI, THOILUONGPHUT, SOCAU, TRANGTHAI, NGAYMO, MATAIKHOAN_TAO)
                          VALUES (@Name, @Duration, @Count, N'Published', GETDATE(), @AccountId);
                          SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tx))
                    {
                        createCmd.Parameters.AddWithValue("@Name", $"[Tự luyện] {className} - {subjectName} - {DateTime.Now:dd/MM HH:mm}");
                        createCmd.Parameters.AddWithValue("@Duration", form.DurationMinutes);
                        createCmd.Parameters.AddWithValue("@Count", form.QuestionCount);
                        createCmd.Parameters.AddWithValue("@AccountId", accountId.Value);
                        examId = Convert.ToInt32(createCmd.ExecuteScalar());
                    }

                    using (var subjectCmd = new SqlCommand(
                        "INSERT INTO DETHI_MONHOC (MADETHI, MAMONHOC) VALUES (@ExamId, @SubjectId)", conn, tx))
                    {
                        subjectCmd.Parameters.AddWithValue("@ExamId", examId);
                        subjectCmd.Parameters.AddWithValue("@SubjectId", subjectId);
                        subjectCmd.ExecuteNonQuery();
                    }

                    using (var classCmd = new SqlCommand(
                        "INSERT INTO DETHI_LOP (MADETHI, MALOP) VALUES (@ExamId, @ClassId)", conn, tx))
                    {
                        classCmd.Parameters.AddWithValue("@ExamId", examId);
                        classCmd.Parameters.AddWithValue("@ClassId", classId);
                        classCmd.ExecuteNonQuery();
                    }

                    using (var assignCmd = new SqlCommand(
                        "INSERT INTO DETHI_HOCSINH (MADETHI, MAHOCSINH) VALUES (@ExamId, @StudentId)", conn, tx))
                    {
                        assignCmd.Parameters.AddWithValue("@ExamId", examId);
                        assignCmd.Parameters.AddWithValue("@StudentId", studentId.Value);
                        assignCmd.ExecuteNonQuery();
                    }

                    for (var i = 0; i < pickedQuestionIds.Count; i++)
                    {
                        using (var questionCmd = new SqlCommand(
                            "INSERT INTO DETHI_CAUHOI (MADETHI, MACAUHOI, STT) VALUES (@ExamId, @QuestionId, @Order)", conn, tx))
                        {
                            questionCmd.Parameters.AddWithValue("@ExamId", examId);
                            questionCmd.Parameters.AddWithValue("@QuestionId", pickedQuestionIds[i]);
                            questionCmd.Parameters.AddWithValue("@Order", i + 1);
                            questionCmd.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                }
            }

            TempData["Message"] = "Đã tạo đề tự luyện thành công.";
            return RedirectToAction("Take", new { id = examId });
        }

        [HttpGet]
        public JsonResult PracticeAvailableCount(int? classId = null, int? subjectId = null)
        {
            if (!classId.HasValue || !subjectId.HasValue)
            {
                return Json(new { count = 0 }, JsonRequestBehavior.AllowGet);
            }

            var count = CountAvailableQuestions(subjectId.Value, classId.Value);
            return Json(new { count }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Take(int id)
        {
            var studentId = Session[SessionKeys.StudentId] as int?;
            if (!studentId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var meta = GetExamMeta(id, studentId.Value);
            if (meta == null)
            {
                return HttpNotFound();
            }

            if (!IsExamAvailable(meta))
            {
                TempData["Message"] = "Đề thi hiện chưa mở hoặc đã đóng.";
                return RedirectToAction("Index");
            }

            var rows = new List<ExamQuestionRow>();
            using (var conn = DbConnectionHelper.CreateSqlConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"SELECT DC.STT,
                             C.MACAUHOI,
                             C.NOIDUNGCAUHOI,
                             D.MADAPAN,
                             D.NOIDUNGDAPAN
                      FROM DETHI_CAUHOI DC
                      INNER JOIN CAUHOI C ON C.MACAUHOI = DC.MACAUHOI
                      INNER JOIN DAPAN D ON D.MACAUHOI = C.MACAUHOI
                      WHERE DC.MADETHI = @ExamId
                        AND D.IsDeleted = 0
                      ORDER BY DC.STT, D.MADAPAN", conn))
                {
                    cmd.Parameters.AddWithValue("@ExamId", id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rows.Add(new ExamQuestionRow
                            {
                                Order = reader.GetInt32(0),
                                QuestionId = reader.GetInt32(1),
                                QuestionText = reader.GetString(2),
                                AnswerId = reader.GetInt32(3),
                                AnswerText = reader.GetString(4)
                            });
                        }
                    }
                }
            }

            if (!rows.Any())
            {
                TempData["Message"] = "Đề thi chưa có dữ liệu câu hỏi.";
                return RedirectToAction("Index");
            }

            var model = new StudentExamTakeViewModel
            {
                ExamId = meta.ExamId,
                ExamName = meta.ExamName,
                DurationMinutes = meta.DurationMinutes,
                Questions = rows
                    .GroupBy(x => new { x.Order, x.QuestionId, x.QuestionText })
                    .OrderBy(x => x.Key.Order)
                    .Select(g => new StudentExamQuestionViewModel
                    {
                        Order = g.Key.Order,
                        QuestionId = g.Key.QuestionId,
                        QuestionText = g.Key.QuestionText,
                        Answers = g.Select(x => new StudentExamAnswerViewModel
                        {
                            AnswerId = x.AnswerId,
                            AnswerText = x.AnswerText
                        }).ToList()
                    })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Submit(StudentExamSubmitViewModel model)
        {
            var studentId = Session[SessionKeys.StudentId] as int?;
            if (!studentId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var meta = GetExamMeta(model.ExamId, studentId.Value);
            if (meta == null || !IsExamAvailable(meta))
            {
                TempData["Message"] = "Đề thi không còn khả dụng.";
                return RedirectToAction("Index");
            }

            var answerRows = new List<ExamAnswerCheckRow>();
            using (var conn = DbConnectionHelper.CreateSqlConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"SELECT DC.STT,
                             C.MACAUHOI,
                             D.MADAPAN,
                             D.DUNGLSAI
                      FROM DETHI_CAUHOI DC
                      INNER JOIN CAUHOI C ON C.MACAUHOI = DC.MACAUHOI
                      INNER JOIN DAPAN D ON D.MACAUHOI = C.MACAUHOI
                      WHERE DC.MADETHI = @ExamId
                        AND D.IsDeleted = 0
                      ORDER BY DC.STT, D.MADAPAN", conn))
                {
                    cmd.Parameters.AddWithValue("@ExamId", model.ExamId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            answerRows.Add(new ExamAnswerCheckRow
                            {
                                Order = reader.GetInt32(0),
                                QuestionId = reader.GetInt32(1),
                                AnswerId = reader.GetInt32(2),
                                IsCorrect = reader.GetBoolean(3)
                            });
                        }
                    }
                }
            }

            var questionGroups = answerRows.GroupBy(x => x.QuestionId).OrderBy(x => x.First().Order).ToList();
            if (!questionGroups.Any())
            {
                TempData["Message"] = "Đề thi chưa có câu hỏi hợp lệ.";
                return RedirectToAction("Index");
            }

            var submitted = (model.Answers ?? new List<StudentExamSubmitAnswerViewModel>())
                .ToDictionary(x => x.QuestionId, x => x.AnswerId);
            var details = new List<ExamSubmitDetailRow>();
            var correctCount = 0;

            foreach (var group in questionGroups)
            {
                int? selectedId;
                submitted.TryGetValue(group.Key, out selectedId);

                var selected = group.FirstOrDefault(x => x.AnswerId == selectedId);
                var isCorrect = selected != null && selected.IsCorrect;
                if (isCorrect)
                {
                    correctCount++;
                }

                details.Add(new ExamSubmitDetailRow
                {
                    QuestionId = group.Key,
                    SelectedAnswerId = selected?.AnswerId,
                    IsCorrect = isCorrect
                });
            }

            var totalCount = questionGroups.Count;
            var score = Math.Round((correctCount * 10.0) / totalCount, 2);

            using (var conn = DbConnectionHelper.CreateSqlConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    int attemptId;
                    using (var insertAttempt = new SqlCommand(
                        @"INSERT INTO DETHI_LANLAM (MADETHI, MAHOCSINH, DIEMSO, SOCAUDUNG, TONGSOCAU)
                          VALUES (@ExamId, @StudentId, @Score, @Correct, @Total);
                          SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tx))
                    {
                        insertAttempt.Parameters.AddWithValue("@ExamId", model.ExamId);
                        insertAttempt.Parameters.AddWithValue("@StudentId", studentId.Value);
                        insertAttempt.Parameters.AddWithValue("@Score", score);
                        insertAttempt.Parameters.AddWithValue("@Correct", correctCount);
                        insertAttempt.Parameters.AddWithValue("@Total", totalCount);
                        attemptId = Convert.ToInt32(insertAttempt.ExecuteScalar());
                    }

                    foreach (var item in details)
                    {
                        using (var insertDetail = new SqlCommand(
                            @"INSERT INTO DETHI_LANLAM_CHITIET (MALANLAM, MACAUHOI, MADAPANCHON, DUNG)
                              VALUES (@AttemptId, @QuestionId, @AnswerId, @IsCorrect)", conn, tx))
                        {
                            insertDetail.Parameters.AddWithValue("@AttemptId", attemptId);
                            insertDetail.Parameters.AddWithValue("@QuestionId", item.QuestionId);
                            insertDetail.Parameters.AddWithValue("@AnswerId", (object)item.SelectedAnswerId ?? DBNull.Value);
                            insertDetail.Parameters.AddWithValue("@IsCorrect", item.IsCorrect);
                            insertDetail.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                }
            }

            TempData["Message"] = $"Nộp đề thành công. Điểm của lần làm này: {score:0.##}/10.";
            return RedirectToAction("History", new { id = model.ExamId });
        }

        public ActionResult History(int id)
        {
            var studentId = Session[SessionKeys.StudentId] as int?;
            if (!studentId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var meta = GetExamMeta(id, studentId.Value);
            if (meta == null)
            {
                return HttpNotFound();
            }

            var attempts = new List<StudentExamAttemptViewModel>();
            using (var conn = DbConnectionHelper.CreateSqlConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"SELECT MALANLAM, DIEMSO, SOCAUDUNG, TONGSOCAU, NGAYNOP
                      FROM DETHI_LANLAM
                      WHERE MADETHI = @ExamId
                        AND MAHOCSINH = @StudentId
                      ORDER BY NGAYNOP DESC", conn))
                {
                    cmd.Parameters.AddWithValue("@ExamId", id);
                    cmd.Parameters.AddWithValue("@StudentId", studentId.Value);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            attempts.Add(new StudentExamAttemptViewModel
                            {
                                AttemptId = reader.GetInt32(0),
                                Score = reader.GetDouble(1),
                                CorrectCount = reader.GetInt32(2),
                                TotalCount = reader.GetInt32(3),
                                SubmitAt = reader.GetDateTime(4)
                            });
                        }
                    }
                }
            }

            var model = new StudentExamHistoryViewModel
            {
                ExamId = meta.ExamId,
                ExamName = meta.ExamName,
                BestScore = attempts.Any() ? attempts.Max(x => x.Score) : (double?)null,
                Attempts = attempts
            };

            return View(model);
        }

        private StudentPracticeCreatePageViewModel BuildPracticePage(StudentPracticeCreateViewModel form, int availableCount)
        {
            form = form ?? new StudentPracticeCreateViewModel();

            return new StudentPracticeCreatePageViewModel
            {
                Form = form,
                ClassOptions = LoadClassOptions(),
                SubjectOptions = LoadSubjectOptions(),
                AvailableQuestionCount = availableCount
            };
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

        private int CountAvailableQuestions(int subjectId, int classId)
        {
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
                        AND B.MAMONHOC = @SubjectId
                        AND B.MALOP = @ClassId", conn))
                {
                    cmd.Parameters.AddWithValue("@SubjectId", subjectId);
                    cmd.Parameters.AddWithValue("@ClassId", classId);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        private ExamMeta GetExamMeta(int examId, int studentId)
        {
            using (var conn = DbConnectionHelper.CreateSqlConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"SELECT E.MADETHI,
                             E.TENDETHI,
                             E.THOILUONGPHUT,
                             E.TRANGTHAI,
                             E.NGAYMO,
                             E.NGAYDONG
                      FROM DETHI E
                      INNER JOIN DETHI_HOCSINH DH ON DH.MADETHI = E.MADETHI
                      WHERE E.MADETHI = @ExamId
                        AND DH.MAHOCSINH = @StudentId", conn))
                {
                    cmd.Parameters.AddWithValue("@ExamId", examId);
                    cmd.Parameters.AddWithValue("@StudentId", studentId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            return null;
                        }

                        return new ExamMeta
                        {
                            ExamId = reader.GetInt32(0),
                            ExamName = reader.GetString(1),
                            DurationMinutes = reader.GetInt32(2),
                            Status = reader.GetString(3),
                            OpenAt = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                            CloseAt = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5)
                        };
                    }
                }
            }
        }

        private static bool IsExamAvailable(ExamMeta meta)
        {
            if (!meta.Status.Equals("Published", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var now = DateTime.Now;
            if (meta.OpenAt.HasValue && now < meta.OpenAt.Value)
            {
                return false;
            }

            if (meta.CloseAt.HasValue && now > meta.CloseAt.Value)
            {
                return false;
            }

            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }

            base.Dispose(disposing);
        }

        private sealed class ExamMeta
        {
            public int ExamId { get; set; }
            public string ExamName { get; set; }
            public int DurationMinutes { get; set; }
            public string Status { get; set; }
            public DateTime? OpenAt { get; set; }
            public DateTime? CloseAt { get; set; }
        }

        private sealed class ExamQuestionRow
        {
            public int Order { get; set; }
            public int QuestionId { get; set; }
            public string QuestionText { get; set; }
            public int AnswerId { get; set; }
            public string AnswerText { get; set; }
        }

        private sealed class ExamAnswerCheckRow
        {
            public int Order { get; set; }
            public int QuestionId { get; set; }
            public int AnswerId { get; set; }
            public bool IsCorrect { get; set; }
        }

        private sealed class ExamSubmitDetailRow
        {
            public int QuestionId { get; set; }
            public int? SelectedAnswerId { get; set; }
            public bool IsCorrect { get; set; }
        }
    }
}
