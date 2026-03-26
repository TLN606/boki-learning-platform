using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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

            var lessonIdSet = new HashSet<int>(lessons.Select(x => x.MABAIHOC));
            var completedCurrentLessons = completedLessonIds.Count(x => lessonIdSet.Contains(x));

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
                CompletedLessons = completedCurrentLessons,
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
