using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using Boki_ELearning.Filters;
using Boki_ELearning.Helpers;
using Boki_ELearning.Models.ViewModels;

namespace Boki_ELearning.Controllers
{
    [SessionAuthorize]
    public class StudentExamController : Controller
    {
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
                                BestScore = reader.IsDBNull(5) ? (double?)null : reader.GetDouble(5),
                                AttemptCount = reader.GetInt32(6),
                                LatestAttemptAt = reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7)
                            });
                        }
                    }
                }
            }

            return View(model);
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
