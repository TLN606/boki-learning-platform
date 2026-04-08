using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Transactions;
using System.Web.Mvc;
using Boki_ELearning.Filters;
using Boki_ELearning.Helpers;
using Boki_ELearning.Models;
using Boki_ELearning.Models.ViewModels;

namespace Boki_ELearning.Controllers
{
    [SessionAuthorize]
    public class LearningController : Controller
    {
        private readonly BokiDBEntities1 _db = new BokiDBEntities1();

        public ActionResult Index()
        {
            var classId = Session[SessionKeys.ClassId] as int?;
            var studentId = Session[SessionKeys.StudentId] as int?;
            var studentName = Session[SessionKeys.StudentName] as string ?? "Học sinh";

            var lessonsQuery = _db.BAIHOCs.AsNoTracking().Where(x => !x.IsDeleted);
            if (classId.HasValue)
            {
                lessonsQuery = lessonsQuery.Where(x => x.MALOP == classId.Value);
            }

            var lessons = lessonsQuery
                .Select(x => new
                {
                    x.MABAIHOC,
                    x.TENBAIHOC,
                    SubjectName = x.MONHOC.TENMONHOC
                })
                .OrderBy(x => x.SubjectName)
                .ThenBy(x => x.TENBAIHOC)
                .ToList();

            var completedLessonIds = new HashSet<int>();
            if (studentId.HasValue)
            {
                completedLessonIds = new HashSet<int>(
                    _db.KETQUAs.AsNoTracking()
                        .Where(x => x.MAHOCSINH == studentId.Value)
                        .Select(x => x.MABAIHOC)
                        .Distinct()
                        .ToList());
            }

            var subjectProgress = lessons
                .GroupBy(x => string.IsNullOrWhiteSpace(x.SubjectName) ? "Khác" : x.SubjectName)
                .Select(g => new SubjectProgressViewModel
                {
                    SubjectName = g.Key,
                    TotalLessons = g.Count(),
                    CompletedLessons = g.Count(x => completedLessonIds.Contains(x.MABAIHOC))
                })
                .OrderBy(x => x.SubjectName)
                .ToList();

            var className = "Chưa gán lớp";
            if (classId.HasValue)
            {
                className = _db.LOPs.AsNoTracking()
                    .Where(x => x.MALOP == classId.Value)
                    .Select(x => x.TENLOP)
                    .FirstOrDefault() ?? className;
            }

            var model = new LearningIndexViewModel
            {
                StudentName = studentName,
                ClassName = className,
                TotalLessons = lessons.Count,
                CompletedLessons = completedLessonIds.Count(x => lessons.Any(y => y.MABAIHOC == x)),
                SubjectProgress = subjectProgress,
                Lessons = lessons.Select(x => new LessonItemViewModel
                {
                    LessonId = x.MABAIHOC,
                    LessonName = x.TENBAIHOC,
                    SubjectName = x.SubjectName,
                    IsCompleted = completedLessonIds.Contains(x.MABAIHOC)
                }).ToList()
            };

            return View(model);
        }

        public ActionResult Lesson(int id)
        {
            var classId = Session[SessionKeys.ClassId] as int?;
            var lesson = _db.BAIHOCs.AsNoTracking()
                .Where(x => x.MABAIHOC == id && !x.IsDeleted)
                .Select(x => new
                {
                    x.MABAIHOC,
                    x.TENBAIHOC,
                    x.NOIDUNGLYTUYET,
                    x.MEOGHINHO,
                    x.MALOP,
                    ClassName = x.LOP.TENLOP,
                    SubjectName = x.MONHOC.TENMONHOC
                })
                .FirstOrDefault();

            if (lesson == null)
            {
                return HttpNotFound();
            }

            if (classId.HasValue && lesson.MALOP != classId.Value)
            {
                return RedirectToAction("Index");
            }

            var questionCount = _db.Database.SqlQuery<int>(
                @"SELECT COUNT(1)
                  FROM CAUHOI
                  WHERE MABAIHOC = @p0
                    AND IsDeleted = 0
                    AND TRANGTHAI = N'Active'",
                id).FirstOrDefault();

            var model = new LessonDetailViewModel
            {
                LessonId = lesson.MABAIHOC,
                LessonName = lesson.TENBAIHOC,
                SubjectName = lesson.SubjectName,
                ClassName = lesson.ClassName,
                TheoryContent = lesson.NOIDUNGLYTUYET,
                TipContent = lesson.MEOGHINHO,
                QuestionCount = questionCount
            };

            return View(model);
        }

        public ActionResult Quiz(int id)
        {
            var classId = Session[SessionKeys.ClassId] as int?;
            var lesson = _db.BAIHOCs.AsNoTracking()
                .Where(x => x.MABAIHOC == id && !x.IsDeleted)
                .Select(x => new { x.MABAIHOC, x.TENBAIHOC, x.MALOP })
                .FirstOrDefault();

            if (lesson == null)
            {
                return HttpNotFound();
            }

            if (classId.HasValue && lesson.MALOP != classId.Value)
            {
                return RedirectToAction("Index");
            }

            var rows = _db.Database.SqlQuery<QuizRow>(
                @"SELECT C.MACAUHOI AS QuestionId,
                         C.NOIDUNGCAUHOI AS QuestionText,
                         D.MADAPAN AS AnswerId,
                         D.NOIDUNGDAPAN AS AnswerText
                  FROM CAUHOI C
                  INNER JOIN DAPAN D ON D.MACAUHOI = C.MACAUHOI
                  WHERE C.MABAIHOC = @p0
                    AND C.IsDeleted = 0
                    AND D.IsDeleted = 0
                    AND C.TRANGTHAI = N'Active'
                  ORDER BY C.MACAUHOI, D.MADAPAN",
                id).ToList();

            if (!rows.Any())
            {
                TempData["Message"] = "Bài học chưa có câu hỏi để làm quiz.";
                return RedirectToAction("Lesson", new { id });
            }

            var questions = rows
                .GroupBy(x => new { x.QuestionId, x.QuestionText })
                .Select(g => new LessonQuizQuestionViewModel
                {
                    QuestionId = g.Key.QuestionId,
                    QuestionText = g.Key.QuestionText,
                    Answers = g.Select(x => new LessonQuizAnswerViewModel
                    {
                        AnswerId = x.AnswerId,
                        AnswerText = x.AnswerText
                    }).ToList()
                })
                .ToList();

            var model = new LessonQuizViewModel
            {
                LessonId = lesson.MABAIHOC,
                LessonName = lesson.TENBAIHOC,
                QuestionCount = questions.Count,
                Questions = questions
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitQuiz(LessonQuizSubmitViewModel model)
        {
            var studentId = Session[SessionKeys.StudentId] as int?;
            if (!studentId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var lesson = _db.BAIHOCs.AsNoTracking()
                .FirstOrDefault(x => x.MABAIHOC == model.LessonId && !x.IsDeleted);
            if (lesson == null)
            {
                return HttpNotFound();
            }

            var answerRows = _db.Database.SqlQuery<AnswerCheckRow>(
                @"SELECT C.MACAUHOI AS QuestionId,
                         D.MADAPAN AS AnswerId,
                         D.DUNGLSAI AS IsCorrect
                  FROM CAUHOI C
                  INNER JOIN DAPAN D ON D.MACAUHOI = C.MACAUHOI
                  WHERE C.MABAIHOC = @p0
                    AND C.IsDeleted = 0
                    AND D.IsDeleted = 0
                    AND C.TRANGTHAI = N'Active'
                  ORDER BY C.MACAUHOI, D.MADAPAN",
                model.LessonId).ToList();

            var questionGroups = answerRows.GroupBy(x => x.QuestionId).ToList();
            if (!questionGroups.Any())
            {
                TempData["Message"] = "Không có dữ liệu câu hỏi cho bài quiz.";
                return RedirectToAction("Lesson", new { id = model.LessonId });
            }

            var submitted = (model.Answers ?? new List<LessonQuizSubmitAnswerViewModel>())
                .ToDictionary(x => x.QuestionId, x => x.AnswerId);

            var detailRows = new List<QuizDetailInsertRow>();
            var correctCount = 0;

            foreach (var group in questionGroups)
            {
                var correctAnswer = group.FirstOrDefault(x => x.IsCorrect);
                var fallbackAnswer = group.FirstOrDefault();
                if (fallbackAnswer == null || correctAnswer == null)
                {
                    continue;
                }

                int? selectedAnswerId;
                submitted.TryGetValue(group.Key, out selectedAnswerId);

                var selected = group.FirstOrDefault(x => x.AnswerId == selectedAnswerId);
                if (selected == null)
                {
                    selected = fallbackAnswer;
                }

                var isCorrect = selected.IsCorrect;
                if (isCorrect)
                {
                    correctCount++;
                }

                detailRows.Add(new QuizDetailInsertRow
                {
                    QuestionId = group.Key,
                    AnswerId = selected.AnswerId,
                    IsCorrect = isCorrect
                });
            }

            var totalCount = questionGroups.Count;
            var score = Math.Round((correctCount * 10.0) / totalCount, 2);

            using (var scope = new TransactionScope())
            {
                var result = new KETQUA
                {
                    MAHOCSINH = studentId.Value,
                    MABAIHOC = model.LessonId,
                    DIEMSO = score,
                    NGAYLAM = DateTime.Now
                };

                _db.KETQUAs.Add(result);
                _db.SaveChanges();

                foreach (var item in detailRows)
                {
                    _db.CHITIKETQUAs.Add(new CHITIKETQUA
                    {
                        MAKETQUA = result.MAKETQUA,
                        MACAUHOI = item.QuestionId,
                        MADAPANCHON = item.AnswerId,
                        DUNG = item.IsCorrect
                    });
                }

                _db.SaveChanges();
                scope.Complete();

                return RedirectToAction("Result", new { id = result.MAKETQUA });
            }
        }

        public ActionResult Result(int id)
        {
            var studentId = Session[SessionKeys.StudentId] as int?;
            if (!studentId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var resultInfo = _db.KETQUAs.AsNoTracking()
                .Where(x => x.MAKETQUA == id && x.MAHOCSINH == studentId.Value)
                .Select(x => new
                {
                    x.MAKETQUA,
                    x.DIEMSO,
                    x.NGAYLAM,
                    LessonName = x.BAIHOC.TENBAIHOC
                })
                .FirstOrDefault();

            if (resultInfo == null)
            {
                return HttpNotFound();
            }

            var detailRows = _db.Database.SqlQuery<ResultRow>(
                @"SELECT C.NOIDUNGCAUHOI AS QuestionText,
                         D.NOIDUNGDAPAN AS SelectedAnswer,
                         CK.DUNG AS IsCorrect
                  FROM CHITIKETQUA CK
                  INNER JOIN CAUHOI C ON C.MACAUHOI = CK.MACAUHOI
                  INNER JOIN DAPAN D ON D.MADAPAN = CK.MADAPANCHON
                  WHERE CK.MAKETQUA = @p0
                  ORDER BY CK.MACHITIKET",
                id).ToList();

            var model = new LessonResultViewModel
            {
                ResultId = resultInfo.MAKETQUA,
                LessonName = resultInfo.LessonName,
                SubmitAt = resultInfo.NGAYLAM,
                Score = resultInfo.DIEMSO,
                TotalCount = detailRows.Count,
                CorrectCount = detailRows.Count(x => x.IsCorrect),
                Questions = detailRows.Select(x => new LessonResultQuestionViewModel
                {
                    QuestionText = x.QuestionText,
                    SelectedAnswer = x.SelectedAnswer,
                    IsCorrect = x.IsCorrect
                }).ToList()
            };

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }

            base.Dispose(disposing);
        }

        private sealed class QuizRow
        {
            public int QuestionId { get; set; }
            public string QuestionText { get; set; }
            public int AnswerId { get; set; }
            public string AnswerText { get; set; }
        }

        private sealed class AnswerCheckRow
        {
            public int QuestionId { get; set; }
            public int AnswerId { get; set; }
            public bool IsCorrect { get; set; }
        }

        private sealed class QuizDetailInsertRow
        {
            public int QuestionId { get; set; }
            public int AnswerId { get; set; }
            public bool IsCorrect { get; set; }
        }

        private sealed class ResultRow
        {
            public string QuestionText { get; set; }
            public string SelectedAnswer { get; set; }
            public bool IsCorrect { get; set; }
        }
    }
}
