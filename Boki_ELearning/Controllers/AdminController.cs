using System;
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
            var totalLearningResults = _db.KETQUAs.Count();
            var averageLearningScore = totalLearningResults > 0
                ? _db.KETQUAs.Average(x => x.DIEMSO)
                : 0;
            var highestLearningScore = totalLearningResults > 0
                ? _db.KETQUAs.Max(x => x.DIEMSO)
                : 0;

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
                TotalLearningResults = totalLearningResults,
                AverageLearningScore = averageLearningScore,
                HighestLearningScore = highestLearningScore,
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

        public ActionResult Results(
            string keyword = null,
            int? classId = null,
            int? subjectId = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            double? minScore = null,
            double? maxScore = null)
        {
            var normalizedKeyword = string.IsNullOrWhiteSpace(keyword) ? null : keyword.Trim();
            var normalizedMinScore = minScore;
            var normalizedMaxScore = maxScore;

            if (normalizedMinScore.HasValue && normalizedMaxScore.HasValue && normalizedMinScore > normalizedMaxScore)
            {
                var temp = normalizedMinScore;
                normalizedMinScore = normalizedMaxScore;
                normalizedMaxScore = temp;
            }

            var query = _db.KETQUAs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(normalizedKeyword))
            {
                query = query.Where(x =>
                    x.HOCSINH.HOTEN.Contains(normalizedKeyword) ||
                    x.BAIHOC.TENBAIHOC.Contains(normalizedKeyword));
            }

            if (classId.HasValue)
            {
                query = query.Where(x => x.HOCSINH.MALOP == classId.Value);
            }

            if (subjectId.HasValue)
            {
                query = query.Where(x => x.BAIHOC.MAMONHOC == subjectId.Value);
            }

            if (fromDate.HasValue)
            {
                var startDate = fromDate.Value.Date;
                query = query.Where(x => x.NGAYLAM >= startDate);
            }

            if (toDate.HasValue)
            {
                var endDateExclusive = toDate.Value.Date.AddDays(1);
                query = query.Where(x => x.NGAYLAM < endDateExclusive);
            }

            if (normalizedMinScore.HasValue)
            {
                query = query.Where(x => x.DIEMSO >= normalizedMinScore.Value);
            }

            if (normalizedMaxScore.HasValue)
            {
                query = query.Where(x => x.DIEMSO <= normalizedMaxScore.Value);
            }

            var totalResults = query.Count();
            var passCount = query.Count(x => x.DIEMSO >= 5);
            var stats = new AdminResultStatsViewModel
            {
                TotalResults = totalResults,
                TotalStudents = query.Select(x => x.MAHOCSINH).Distinct().Count(),
                AverageScore = totalResults > 0 ? query.Average(x => x.DIEMSO) : 0,
                HighestScore = totalResults > 0 ? query.Max(x => x.DIEMSO) : 0,
                LowestScore = totalResults > 0 ? query.Min(x => x.DIEMSO) : 0,
                PassCount = passCount,
                FailCount = totalResults - passCount,
                PassRate = totalResults > 0 ? (double)passCount * 100d / totalResults : 0
            };

            var results = query
                .OrderByDescending(x => x.NGAYLAM)
                .Take(300)
                .Select(x => new AdminResultListItemViewModel
                {
                    ResultId = x.MAKETQUA,
                    StudentName = x.HOCSINH.HOTEN,
                    ClassName = x.HOCSINH.LOP.TENLOP,
                    LessonName = x.BAIHOC.TENBAIHOC,
                    SubjectName = x.BAIHOC.MONHOC.TENMONHOC,
                    Score = x.DIEMSO,
                    SubmitAt = x.NGAYLAM
                })
                .ToList();

            var model = new AdminResultsPageViewModel
            {
                Filter = new AdminResultFilterViewModel
                {
                    Keyword = normalizedKeyword,
                    ClassId = classId,
                    SubjectId = subjectId,
                    FromDate = fromDate,
                    ToDate = toDate,
                    MinScore = normalizedMinScore,
                    MaxScore = normalizedMaxScore
                },
                ClassOptions = _db.LOPs
                    .OrderBy(x => x.TENLOP)
                    .Select(x => new AdminOptionViewModel
                    {
                        Id = x.MALOP,
                        Name = x.TENLOP
                    })
                    .ToList(),
                SubjectOptions = _db.MONHOCs
                    .OrderBy(x => x.TENMONHOC)
                    .Select(x => new AdminOptionViewModel
                    {
                        Id = x.MAMONHOC,
                        Name = x.TENMONHOC
                    })
                    .ToList(),
                Stats = stats,
                Results = results
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
