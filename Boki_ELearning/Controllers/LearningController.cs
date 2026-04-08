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

        public ActionResult Index(int? classId = null, int? subjectId = null, bool? hasQuiz = null)
        {
            var studentId = Session[SessionKeys.StudentId] as int?;
            var suggestedClassId = Session[SessionKeys.ClassId] as int?;
            var studentName = Session[SessionKeys.StudentName] as string ?? "Học sinh";

            var lessons = _db.BAIHOCs.AsNoTracking()
                .Where(x => !x.IsDeleted)
                .Select(x => new
                {
                    x.MABAIHOC,
                    x.TENBAIHOC,
                    x.MALOP,
                    x.MAMONHOC,
                    ClassName = x.LOP.TENLOP,
                    SubjectName = x.MONHOC.TENMONHOC
                })
                .ToList();

            var questionCountMap = _db.Database.SqlQuery<LessonQuestionCountRow>(
                    @"SELECT C.MABAIHOC AS LessonId,
                             COUNT(1) AS QuestionCount
                      FROM CAUHOI C
                      WHERE C.IsDeleted = 0
                        AND C.TRANGTHAI = N'Active'
                      GROUP BY C.MABAIHOC")
                .ToDictionary(x => x.LessonId, x => x.QuestionCount);

            var lessonRows = lessons
                .Select(x => new LessonListRow
                {
                    LessonId = x.MABAIHOC,
                    LessonName = x.TENBAIHOC,
                    ClassId = x.MALOP,
                    ClassName = x.ClassName,
                    SubjectId = x.MAMONHOC,
                    SubjectName = x.SubjectName,
                    QuestionCount = questionCountMap.ContainsKey(x.MABAIHOC) ? questionCountMap[x.MABAIHOC] : 0
                })
                .ToList();

            if (classId.HasValue)
            {
                lessonRows = lessonRows.Where(x => x.ClassId == classId.Value).ToList();
            }

            if (subjectId.HasValue)
            {
                lessonRows = lessonRows.Where(x => x.SubjectId == subjectId.Value).ToList();
            }

            if (hasQuiz.HasValue)
            {
                lessonRows = hasQuiz.Value
                    ? lessonRows.Where(x => x.QuestionCount > 0).ToList()
                    : lessonRows.Where(x => x.QuestionCount == 0).ToList();
            }

            lessonRows = lessonRows
                .OrderBy(x => x.ClassName)
                .ThenBy(x => x.SubjectName)
                .ThenBy(x => x.LessonName)
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

            var subjectProgress = lessonRows
                .GroupBy(x => string.IsNullOrWhiteSpace(x.SubjectName) ? "Khác" : x.SubjectName)
                .Select(g => new SubjectProgressViewModel
                {
                    SubjectName = g.Key,
                    TotalLessons = g.Count(),
                    CompletedLessons = g.Count(x => completedLessonIds.Contains(x.LessonId))
                })
                .OrderBy(x => x.SubjectName)
                .ToList();

            var className = "Chưa gán lớp";
            if (suggestedClassId.HasValue)
            {
                className = _db.LOPs.AsNoTracking()
                    .Where(x => x.MALOP == suggestedClassId.Value)
                    .Select(x => x.TENLOP)
                    .FirstOrDefault() ?? className;
            }

            var classOptions = _db.LOPs.AsNoTracking()
                .OrderBy(x => x.TENLOP)
                .Select(x => new LearningFilterOptionViewModel
                {
                    Id = x.MALOP,
                    Name = x.TENLOP
                })
                .ToList();

            var subjectOptions = _db.MONHOCs.AsNoTracking()
                .OrderBy(x => x.TENMONHOC)
                .Select(x => new LearningFilterOptionViewModel
                {
                    Id = x.MAMONHOC,
                    Name = x.TENMONHOC
                })
                .ToList();

            var model = new LearningIndexViewModel
            {
                StudentName = studentName,
                ClassName = className,
                SuggestedClassId = suggestedClassId,
                TotalLessons = lessonRows.Count,
                CompletedLessons = lessonRows.Count(x => completedLessonIds.Contains(x.LessonId)),
                LessonsWithQuiz = lessonRows.Count(x => x.QuestionCount > 0),
                LessonsWithoutQuiz = lessonRows.Count(x => x.QuestionCount == 0),
                SelectedClassId = classId,
                SelectedSubjectId = subjectId,
                SelectedHasQuiz = hasQuiz,
                ClassOptions = classOptions,
                SubjectOptions = subjectOptions,
                SubjectProgress = subjectProgress,
                Lessons = lessonRows.Select(x => new LessonItemViewModel
                {
                    LessonId = x.LessonId,
                    LessonName = x.LessonName,
                    ClassName = x.ClassName,
                    SubjectName = x.SubjectName,
                    QuestionCount = x.QuestionCount,
                    IsCompleted = completedLessonIds.Contains(x.LessonId)
                }).ToList()
            };

            return View(model);
        }

        public ActionResult Lesson(int id)
        {
            var lesson = _db.BAIHOCs.AsNoTracking()
                .Where(x => x.MABAIHOC == id && !x.IsDeleted)
                .Select(x => new
                {
                    x.MABAIHOC,
                    x.TENBAIHOC,
                    x.NOIDUNGLYTUYET,
                    x.MEOGHINHO,
                    ClassName = x.LOP.TENLOP,
                    SubjectName = x.MONHOC.TENMONHOC
                })
                .FirstOrDefault();

            if (lesson == null)
            {
                return HttpNotFound();
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
            var lesson = _db.BAIHOCs.AsNoTracking()
                .Where(x => x.MABAIHOC == id && !x.IsDeleted)
                .Select(x => new { x.MABAIHOC, x.TENBAIHOC })
                .FirstOrDefault();

            if (lesson == null)
            {
                return HttpNotFound();
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

        private sealed class LessonQuestionCountRow
        {
            public int LessonId { get; set; }
            public int QuestionCount { get; set; }
        }

        private sealed class LessonListRow
        {
            public int LessonId { get; set; }
            public string LessonName { get; set; }
            public int ClassId { get; set; }
            public string ClassName { get; set; }
            public int SubjectId { get; set; }
            public string SubjectName { get; set; }
            public int QuestionCount { get; set; }
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
