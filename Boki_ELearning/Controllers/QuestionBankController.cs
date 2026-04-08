using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Boki_ELearning.Filters;
using Boki_ELearning.Helpers;
using Boki_ELearning.Models;
using Boki_ELearning.Models.ViewModels;
using Newtonsoft.Json;

namespace Boki_ELearning.Controllers
{
    [AdminAuthorize]
    public class QuestionBankController : Controller
    {
        private readonly BokiDBEntities1 _db = new BokiDBEntities1();

        public ActionResult Index(string keyword = null, int? lessonId = null, string difficulty = null, string status = null, int? editId = null)
        {
            var filter = new QuestionBankFilterViewModel
            {
                Keyword = keyword,
                LessonId = lessonId,
                Difficulty = difficulty,
                Status = status
            };

            var editor = editId.HasValue ? LoadEditor(editId.Value) : CreateDefaultEditor();
            var model = new QuestionBankPageViewModel
            {
                Filter = filter,
                Editor = editor,
                LessonOptions = LoadLessonOptions(),
                Questions = LoadQuestionList(filter)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Save(QuestionBankEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var page = new QuestionBankPageViewModel
                {
                    Filter = new QuestionBankFilterViewModel(),
                    Editor = model,
                    LessonOptions = LoadLessonOptions(),
                    Questions = LoadQuestionList(new QuestionBankFilterViewModel())
                };
                return View("Index", page);
            }

            var normalizedDifficulty = NormalizeDifficulty(model.Difficulty);
            var normalizedStatus = NormalizeStatus(model.Status);

            using (var conn = DbConnectionHelper.CreateSqlConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    int questionId;
                    int subjectId;
                    using (var lessonCmd = new SqlCommand(
                        @"SELECT B.MAMONHOC
                          FROM BAIHOC B
                          WHERE B.MABAIHOC = @LessonId
                            AND B.IsDeleted = 0", conn, tx))
                    {
                        lessonCmd.Parameters.AddWithValue("@LessonId", model.LessonId);
                        var lessonResult = lessonCmd.ExecuteScalar();
                        if (lessonResult == null)
                        {
                            tx.Rollback();
                            ModelState.AddModelError("LessonId", "Bài học không tồn tại hoặc đã bị ẩn.");
                            return RedirectToAction("Index");
                        }

                        subjectId = Convert.ToInt32(lessonResult);
                    }

                    if (model.QuestionId.HasValue)
                    {
                        questionId = model.QuestionId.Value;
                        using (var updateCmd = new SqlCommand(
                            @"UPDATE CAUHOI
                              SET NOIDUNGCAUHOI = @QuestionText,
                                  MABAIHOC = @LessonId,
                                  MUCDO = @Difficulty,
                                  TRANGTHAI = @Status
                              WHERE MACAUHOI = @QuestionId
                                AND IsDeleted = 0", conn, tx))
                        {
                            updateCmd.Parameters.AddWithValue("@QuestionId", questionId);
                            updateCmd.Parameters.AddWithValue("@QuestionText", model.QuestionText.Trim());
                            updateCmd.Parameters.AddWithValue("@LessonId", model.LessonId);
                            updateCmd.Parameters.AddWithValue("@Difficulty", normalizedDifficulty);
                            updateCmd.Parameters.AddWithValue("@Status", normalizedStatus);
                            var affected = updateCmd.ExecuteNonQuery();
                            if (affected == 0)
                            {
                                tx.Rollback();
                                TempData["Message"] = "Không thể cập nhật câu hỏi.";
                                return RedirectToAction("Index");
                            }
                        }

                        using (var hideAnswersCmd = new SqlCommand(
                            @"UPDATE DAPAN
                              SET IsDeleted = 1
                              WHERE MACAUHOI = @QuestionId
                                AND IsDeleted = 0", conn, tx))
                        {
                            hideAnswersCmd.Parameters.AddWithValue("@QuestionId", questionId);
                            hideAnswersCmd.ExecuteNonQuery();
                        }
                    }
                    else
                    {
                        using (var insertCmd = new SqlCommand(
                            @"INSERT INTO CAUHOI (NOIDUNGCAUHOI, MABAIHOC, IsDeleted, MUCDO, TRANGTHAI)
                              VALUES (@QuestionText, @LessonId, 0, @Difficulty, @Status);
                              SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tx))
                        {
                            insertCmd.Parameters.AddWithValue("@QuestionText", model.QuestionText.Trim());
                            insertCmd.Parameters.AddWithValue("@LessonId", model.LessonId);
                            insertCmd.Parameters.AddWithValue("@Difficulty", normalizedDifficulty);
                            insertCmd.Parameters.AddWithValue("@Status", normalizedStatus);
                            questionId = Convert.ToInt32(insertCmd.ExecuteScalar());
                        }
                    }

                    var answers = new[] { model.AnswerA, model.AnswerB, model.AnswerC, model.AnswerD };
                    for (var i = 0; i < answers.Length; i++)
                    {
                        using (var answerCmd = new SqlCommand(
                            @"INSERT INTO DAPAN (NOIDUNGDAPAN, DUNGLSAI, MAMONHOC, MACAUHOI, IsDeleted)
                              VALUES (@Text, @IsCorrect, @SubjectId, @QuestionId, 0)", conn, tx))
                        {
                            answerCmd.Parameters.AddWithValue("@Text", (answers[i] ?? string.Empty).Trim());
                            answerCmd.Parameters.AddWithValue("@IsCorrect", i == model.CorrectIndex ? 1 : 0);
                            answerCmd.Parameters.AddWithValue("@SubjectId", subjectId);
                            answerCmd.Parameters.AddWithValue("@QuestionId", questionId);
                            answerCmd.ExecuteNonQuery();
                        }
                    }

                    tx.Commit();
                }
            }

            TempData["Message"] = model.QuestionId.HasValue ? "Cập nhật câu hỏi thành công." : "Tạo câu hỏi thành công.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            using (var conn = DbConnectionHelper.CreateSqlConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    using (var qCmd = new SqlCommand(
                        @"UPDATE CAUHOI
                          SET IsDeleted = 1, TRANGTHAI = N'Draft'
                          WHERE MACAUHOI = @QuestionId", conn, tx))
                    {
                        qCmd.Parameters.AddWithValue("@QuestionId", id);
                        qCmd.ExecuteNonQuery();
                    }

                    using (var aCmd = new SqlCommand(
                        @"UPDATE DAPAN
                          SET IsDeleted = 1
                          WHERE MACAUHOI = @QuestionId", conn, tx))
                    {
                        aCmd.Parameters.AddWithValue("@QuestionId", id);
                        aCmd.ExecuteNonQuery();
                    }

                    tx.Commit();
                }
            }

            TempData["Message"] = "Đã ẩn câu hỏi khỏi ngân hàng.";
            return RedirectToAction("Index");
        }

        public ActionResult Export()
        {
            var items = new List<QuestionBankImportItemViewModel>();

            using (var conn = DbConnectionHelper.CreateSqlConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(
                    @"SELECT C.MACAUHOI,
                             C.MABAIHOC,
                             C.NOIDUNGCAUHOI,
                             C.MUCDO,
                             C.TRANGTHAI,
                             D.MADAPAN,
                             D.NOIDUNGDAPAN,
                             D.DUNGLSAI
                      FROM CAUHOI C
                      INNER JOIN DAPAN D ON D.MACAUHOI = C.MACAUHOI
                      WHERE C.IsDeleted = 0
                        AND D.IsDeleted = 0
                      ORDER BY C.MACAUHOI, D.MADAPAN", conn);
                using (var reader = cmd.ExecuteReader())
                {
                    var map = new Dictionary<int, QuestionExportRow>();
                    while (reader.Read())
                    {
                        var questionId = reader.GetInt32(0);
                        QuestionExportRow row;
                        if (!map.TryGetValue(questionId, out row))
                        {
                            row = new QuestionExportRow
                            {
                                LessonId = reader.GetInt32(1),
                                QuestionText = reader.GetString(2),
                                Difficulty = reader.GetString(3),
                                Status = reader.GetString(4),
                                Answers = new List<AnswerExportRow>()
                            };
                            map[questionId] = row;
                        }

                        row.Answers.Add(new AnswerExportRow
                        {
                            Text = reader.GetString(6),
                            IsCorrect = reader.GetBoolean(7)
                        });
                    }

                    foreach (var row in map.Values)
                    {
                        if (row.Answers.Count < 4)
                        {
                            continue;
                        }

                        var answerTexts = row.Answers.Take(4).Select(x => x.Text).ToList();
                        var correctIndex = row.Answers.Take(4).ToList().FindIndex(x => x.IsCorrect);
                        if (correctIndex < 0)
                        {
                            correctIndex = 0;
                        }

                        items.Add(new QuestionBankImportItemViewModel
                        {
                            LessonId = row.LessonId,
                            QuestionText = row.QuestionText,
                            Difficulty = row.Difficulty,
                            Status = row.Status,
                            AnswerA = answerTexts.ElementAtOrDefault(0) ?? string.Empty,
                            AnswerB = answerTexts.ElementAtOrDefault(1) ?? string.Empty,
                            AnswerC = answerTexts.ElementAtOrDefault(2) ?? string.Empty,
                            AnswerD = answerTexts.ElementAtOrDefault(3) ?? string.Empty,
                            CorrectIndex = correctIndex
                        });
                    }
                }
            }

            var json = JsonConvert.SerializeObject(items, Formatting.Indented);
            var bytes = Encoding.UTF8.GetBytes(json);
            return File(bytes, "application/json", "question-bank-export.json");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Import(HttpPostedFileBase file)
        {
            if (file == null || file.ContentLength <= 0)
            {
                TempData["Message"] = "Vui lòng chọn file JSON để import.";
                return RedirectToAction("Index");
            }

            List<QuestionBankImportItemViewModel> items;
            try
            {
                using (var sr = new StreamReader(file.InputStream, Encoding.UTF8))
                {
                    var json = sr.ReadToEnd();
                    items = JsonConvert.DeserializeObject<List<QuestionBankImportItemViewModel>>(json)
                        ?? new List<QuestionBankImportItemViewModel>();
                }
            }
            catch (Exception)
            {
                TempData["Message"] = "File JSON không hợp lệ.";
                return RedirectToAction("Index");
            }

            if (!items.Any())
            {
                TempData["Message"] = "File không có dữ liệu hợp lệ.";
                return RedirectToAction("Index");
            }

            var inserted = 0;
            using (var conn = DbConnectionHelper.CreateSqlConnection())
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    foreach (var item in items)
                    {
                        if (item.LessonId <= 0 || string.IsNullOrWhiteSpace(item.QuestionText))
                        {
                            continue;
                        }

                        int subjectId;
                        using (var lessonCmd = new SqlCommand(
                            @"SELECT MAMONHOC
                              FROM BAIHOC
                              WHERE MABAIHOC = @LessonId
                                AND IsDeleted = 0", conn, tx))
                        {
                            lessonCmd.Parameters.AddWithValue("@LessonId", item.LessonId);
                            var subjectValue = lessonCmd.ExecuteScalar();
                            if (subjectValue == null)
                            {
                                continue;
                            }

                            subjectId = Convert.ToInt32(subjectValue);
                        }

                        int questionId;
                        using (var insertCmd = new SqlCommand(
                            @"INSERT INTO CAUHOI (NOIDUNGCAUHOI, MABAIHOC, IsDeleted, MUCDO, TRANGTHAI)
                              VALUES (@QuestionText, @LessonId, 0, @Difficulty, @Status);
                              SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tx))
                        {
                            insertCmd.Parameters.AddWithValue("@QuestionText", item.QuestionText.Trim());
                            insertCmd.Parameters.AddWithValue("@LessonId", item.LessonId);
                            insertCmd.Parameters.AddWithValue("@Difficulty", NormalizeDifficulty(item.Difficulty));
                            insertCmd.Parameters.AddWithValue("@Status", NormalizeStatus(item.Status));
                            questionId = Convert.ToInt32(insertCmd.ExecuteScalar());
                        }

                        var answers = new[] { item.AnswerA, item.AnswerB, item.AnswerC, item.AnswerD };
                        for (var i = 0; i < answers.Length; i++)
                        {
                            using (var answerCmd = new SqlCommand(
                                @"INSERT INTO DAPAN (NOIDUNGDAPAN, DUNGLSAI, MAMONHOC, MACAUHOI, IsDeleted)
                                  VALUES (@Text, @IsCorrect, @SubjectId, @QuestionId, 0)", conn, tx))
                            {
                                answerCmd.Parameters.AddWithValue("@Text", (answers[i] ?? string.Empty).Trim());
                                answerCmd.Parameters.AddWithValue("@IsCorrect", i == item.CorrectIndex ? 1 : 0);
                                answerCmd.Parameters.AddWithValue("@SubjectId", subjectId);
                                answerCmd.Parameters.AddWithValue("@QuestionId", questionId);
                                answerCmd.ExecuteNonQuery();
                            }
                        }

                        inserted++;
                    }

                    tx.Commit();
                }
            }

            TempData["Message"] = $"Import thành công {inserted} câu hỏi.";
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }

            base.Dispose(disposing);
        }

        private List<QuestionBankLessonOptionViewModel> LoadLessonOptions()
        {
            return _db.BAIHOCs
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.LOP.TENLOP)
                .ThenBy(x => x.MONHOC.TENMONHOC)
                .ThenBy(x => x.TENBAIHOC)
                .Select(x => new QuestionBankLessonOptionViewModel
                {
                    LessonId = x.MABAIHOC,
                    LessonDisplay = x.LOP.TENLOP + " - " + x.MONHOC.TENMONHOC + " - " + x.TENBAIHOC
                })
                .ToList();
        }

        private List<QuestionBankListItemViewModel> LoadQuestionList(QuestionBankFilterViewModel filter)
        {
            var list = new List<QuestionBankListItemViewModel>();
            using (var conn = DbConnectionHelper.CreateSqlConnection())
            {
                conn.Open();

                var sql = new StringBuilder(@"
                    SELECT C.MACAUHOI,
                           C.MABAIHOC,
                           C.NOIDUNGCAUHOI,
                           C.MUCDO,
                           C.TRANGTHAI,
                           B.TENBAIHOC,
                           M.TENMONHOC,
                           L.TENLOP,
                           (SELECT COUNT(1) FROM DAPAN D WHERE D.MACAUHOI = C.MACAUHOI AND D.IsDeleted = 0) AS AnswerCount
                    FROM CAUHOI C
                    INNER JOIN BAIHOC B ON B.MABAIHOC = C.MABAIHOC
                    INNER JOIN MONHOC M ON M.MAMONHOC = B.MAMONHOC
                    INNER JOIN LOP L ON L.MALOP = B.MALOP
                    WHERE C.IsDeleted = 0");

                var cmd = new SqlCommand();
                cmd.Connection = conn;

                if (!string.IsNullOrWhiteSpace(filter.Keyword))
                {
                    sql.Append(" AND C.NOIDUNGCAUHOI LIKE @Keyword");
                    cmd.Parameters.AddWithValue("@Keyword", "%" + filter.Keyword.Trim() + "%");
                }

                if (filter.LessonId.HasValue)
                {
                    sql.Append(" AND C.MABAIHOC = @LessonId");
                    cmd.Parameters.AddWithValue("@LessonId", filter.LessonId.Value);
                }

                if (!string.IsNullOrWhiteSpace(filter.Difficulty))
                {
                    sql.Append(" AND C.MUCDO = @Difficulty");
                    cmd.Parameters.AddWithValue("@Difficulty", NormalizeDifficulty(filter.Difficulty));
                }

                if (!string.IsNullOrWhiteSpace(filter.Status))
                {
                    sql.Append(" AND C.TRANGTHAI = @Status");
                    cmd.Parameters.AddWithValue("@Status", NormalizeStatus(filter.Status));
                }

                sql.Append(" ORDER BY C.MACAUHOI DESC");
                cmd.CommandText = sql.ToString();

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new QuestionBankListItemViewModel
                        {
                            QuestionId = reader.GetInt32(0),
                            LessonId = reader.GetInt32(1),
                            QuestionText = reader.GetString(2),
                            Difficulty = reader.GetString(3),
                            Status = reader.GetString(4),
                            LessonName = reader.GetString(5),
                            SubjectName = reader.GetString(6),
                            ClassName = reader.GetString(7),
                            AnswerCount = reader.GetInt32(8)
                        });
                    }
                }
            }

            return list;
        }

        private QuestionBankEditViewModel LoadEditor(int questionId)
        {
            using (var conn = DbConnectionHelper.CreateSqlConnection())
            {
                conn.Open();

                var cmd = new SqlCommand(
                    @"SELECT C.MACAUHOI,
                             C.MABAIHOC,
                             C.NOIDUNGCAUHOI,
                             C.MUCDO,
                             C.TRANGTHAI,
                             D.NOIDUNGDAPAN,
                             D.DUNGLSAI
                      FROM CAUHOI C
                      INNER JOIN DAPAN D ON D.MACAUHOI = C.MACAUHOI
                      WHERE C.MACAUHOI = @QuestionId
                        AND C.IsDeleted = 0
                        AND D.IsDeleted = 0
                      ORDER BY D.MADAPAN", conn);
                cmd.Parameters.AddWithValue("@QuestionId", questionId);

                using (var reader = cmd.ExecuteReader())
                {
                    var answers = new List<string>();
                    var correctIndex = 0;
                    int? lessonId = null;
                    string questionText = null;
                    string difficulty = null;
                    string status = null;
                    var idx = 0;

                    while (reader.Read())
                    {
                        if (!lessonId.HasValue)
                        {
                            lessonId = reader.GetInt32(1);
                            questionText = reader.GetString(2);
                            difficulty = reader.GetString(3);
                            status = reader.GetString(4);
                        }

                        answers.Add(reader.GetString(5));
                        if (reader.GetBoolean(6))
                        {
                            correctIndex = idx;
                        }

                        idx++;
                    }

                    if (!lessonId.HasValue)
                    {
                        return CreateDefaultEditor();
                    }

                    while (answers.Count < 4)
                    {
                        answers.Add(string.Empty);
                    }

                    return new QuestionBankEditViewModel
                    {
                        QuestionId = questionId,
                        LessonId = lessonId.Value,
                        QuestionText = questionText,
                        Difficulty = difficulty,
                        Status = status,
                        AnswerA = answers[0],
                        AnswerB = answers[1],
                        AnswerC = answers[2],
                        AnswerD = answers[3],
                        CorrectIndex = correctIndex
                    };
                }
            }
        }

        private static QuestionBankEditViewModel CreateDefaultEditor()
        {
            return new QuestionBankEditViewModel
            {
                Difficulty = "TrungBinh",
                Status = "Active",
                CorrectIndex = 0
            };
        }

        private static string NormalizeDifficulty(string difficulty)
        {
            var value = (difficulty ?? string.Empty).Trim();
            if (value.Equals("De", StringComparison.OrdinalIgnoreCase))
            {
                return "De";
            }

            if (value.Equals("Kho", StringComparison.OrdinalIgnoreCase))
            {
                return "Kho";
            }

            return "TrungBinh";
        }

        private static string NormalizeStatus(string status)
        {
            var value = (status ?? string.Empty).Trim();
            return value.Equals("Draft", StringComparison.OrdinalIgnoreCase) ? "Draft" : "Active";
        }

        private sealed class QuestionExportRow
        {
            public int LessonId { get; set; }
            public string QuestionText { get; set; }
            public string Difficulty { get; set; }
            public string Status { get; set; }
            public List<AnswerExportRow> Answers { get; set; }
        }

        private sealed class AnswerExportRow
        {
            public string Text { get; set; }
            public bool IsCorrect { get; set; }
        }
    }
}
