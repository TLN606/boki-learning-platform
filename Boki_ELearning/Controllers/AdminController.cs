using System.Linq;
using System.Web.Mvc;
using Boki_ELearning.Filters;
using Boki_ELearning.Models;
using Boki_ELearning.Models.ViewModels;

namespace Boki_ELearning.Controllers
{
    [AdminAuthorize]
    public class AdminController : Controller
    {
        private readonly BokiDBEntities1 _db = new BokiDBEntities1();

        public ActionResult Dashboard()
        {
            var model = new AdminDashboardViewModel
            {
                TotalStudents = _db.HOCSINHs.Count(),
                TotalLessons = _db.BAIHOCs.Count(x => !x.IsDeleted),
                TotalQuestions = _db.Database.SqlQuery<int>(
                    "SELECT COUNT(1) FROM CAUHOI WHERE IsDeleted = 0 AND TRANGTHAI = N'Active'")
                    .FirstOrDefault(),
                TotalExamAttempts = _db.Database.SqlQuery<int>(
                    "SELECT COUNT(1) FROM DETHI_LANLAM")
                    .FirstOrDefault(),
                TotalQuizAttempts = _db.KETQUAs.Count(),
                RecentStudents = _db.HOCSINHs
                    .OrderByDescending(x => x.MAHOCSINH)
                    .Take(8)
                    .Select(x => new AdminRecentStudentViewModel
                    {
                        FullName = x.HOTEN,
                        ClassName = x.LOP.TENLOP,
                        Email = x.TAIKHOAN.EMAIL
                    })
                    .ToList(),
                RecentResults = _db.KETQUAs
                    .OrderByDescending(x => x.NGAYLAM)
                    .Take(8)
                    .Select(x => new AdminRecentResultViewModel
                    {
                        StudentName = x.HOCSINH.HOTEN,
                        LessonName = x.BAIHOC.TENBAIHOC,
                        Score = x.DIEMSO,
                        SubmitAt = x.NGAYLAM
                    })
                    .ToList(),
                LessonProgress = _db.KETQUAs
                    .GroupBy(x => new { x.BAIHOC.TENBAIHOC, x.BAIHOC.MONHOC.TENMONHOC, x.BAIHOC.LOP.TENLOP })
                    .Select(g => new AdminLessonProgressViewModel
                    {
                        LessonName = g.Key.TENBAIHOC,
                        SubjectName = g.Key.TENMONHOC,
                        ClassName = g.Key.TENLOP,
                        StudentCount = g.Select(x => x.MAHOCSINH).Distinct().Count(),
                        AttemptCount = g.Count(),
                        AvgScore = g.Average(x => x.DIEMSO)
                    })
                    .OrderBy(x => x.ClassName)
                    .ThenBy(x => x.SubjectName)
                    .ThenBy(x => x.LessonName)
                    .ToList(),
                ClassProgress = _db.LOPs
                    .Select(l => new AdminClassProgressViewModel
                    {
                        ClassName = l.TENLOP,
                        StudentCount = l.HOCSINHs.Count(),
                        LessonsCompleted = l.HOCSINHs.SelectMany(s => s.KETQUAs).Select(k => k.MABAIHOC).Distinct().Count(),
                        TotalAttempts = l.HOCSINHs.SelectMany(s => s.KETQUAs).Count(),
                        AvgScore = l.HOCSINHs.SelectMany(s => s.KETQUAs).Any()
                            ? l.HOCSINHs.SelectMany(s => s.KETQUAs).Average(k => k.DIEMSO)
                            : 0
                    })
                    .OrderBy(x => x.ClassName)
                    .ToList()
            };

            return View(model);
        }

        public ActionResult Students()
        {
            var model = _db.HOCSINHs
                .OrderBy(x => x.LOP.TENLOP)
                .ThenBy(x => x.HOTEN)
                .Select(x => new AdminStudentListItemViewModel
                {
                    StudentId = x.MAHOCSINH,
                    FullName = x.HOTEN,
                    ClassName = x.LOP.TENLOP,
                    Email = x.TAIKHOAN.EMAIL,
                    AttemptCount = x.KETQUAs.Count()
                })
                .ToList();

            return View(model);
        }

        public ActionResult Results()
        {
            var model = _db.KETQUAs
                .OrderByDescending(x => x.NGAYLAM)
                .Take(300)
                .Select(x => new AdminResultListItemViewModel
                {
                    ResultId = x.MAKETQUA,
                    StudentName = x.HOCSINH.HOTEN,
                    LessonName = x.BAIHOC.TENBAIHOC,
                    SubjectName = x.BAIHOC.MONHOC.TENMONHOC,
                    Score = x.DIEMSO,
                    SubmitAt = x.NGAYLAM
                })
                .ToList();

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
