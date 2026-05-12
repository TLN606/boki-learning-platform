using System;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Boki_ELearning.Filters;
using Boki_ELearning.Models;
using Boki_ELearning.Models.ViewModels;

namespace Boki_ELearning.Controllers
{
    [AdminAuthorize]
    public class AdminLessonController : Controller
    {
        private readonly BokiDBEntities1 _db = new BokiDBEntities1();

        public ActionResult Index(int? classId = null, int? subjectId = null)
        {
            ViewBag.AdminNav = "lessons";

            var query = _db.BAIHOCs.AsNoTracking().Where(x => !x.IsDeleted);

            if (classId.HasValue)
            {
                query = query.Where(x => x.MALOP == classId.Value);
            }

            if (subjectId.HasValue)
            {
                query = query.Where(x => x.MAMONHOC == subjectId.Value);
            }

            var questionCounts = _db.Database.SqlQuery<LessonQuestionCountDto>(
                @"SELECT MABAIHOC AS LessonId, COUNT(1) AS QuestionCount
                  FROM CAUHOI
                  WHERE IsDeleted = 0 AND TRANGTHAI = N'Active'
                  GROUP BY MABAIHOC").ToDictionary(x => x.LessonId, x => x.QuestionCount);

            var lessons = query
                .OrderBy(x => x.LOP.TENLOP)
                .ThenBy(x => x.MONHOC.TENMONHOC)
                .ThenBy(x => x.TENBAIHOC)
                .Select(x => new AdminLessonListItemViewModel
                {
                    LessonId = x.MABAIHOC,
                    LessonName = x.TENBAIHOC,
                    SubjectName = x.MONHOC.TENMONHOC,
                    ClassName = x.LOP.TENLOP,
                    IsDeleted = x.IsDeleted
                })
                .ToList();

            foreach (var lesson in lessons)
            {
                int count;
                if (questionCounts.TryGetValue(lesson.LessonId, out count))
                {
                    lesson.QuestionCount = count;
                }
            }

            var model = new AdminLessonListViewModel
            {
                Lessons = lessons,
                FilterClassId = classId,
                FilterSubjectId = subjectId,
                ClassOptions = _db.LOPs.AsNoTracking()
                    .OrderBy(x => x.TENLOP)
                    .Select(x => new SelectOptionViewModel { Id = x.MALOP, Name = x.TENLOP })
                    .ToList(),
                SubjectOptions = _db.MONHOCs.AsNoTracking()
                    .OrderBy(x => x.TENMONHOC)
                    .Select(x => new SelectOptionViewModel { Id = x.MAMONHOC, Name = x.TENMONHOC })
                    .ToList()
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult Create()
        {
            ViewBag.AdminNav = "lessons";

            var model = new AdminLessonEditViewModel
            {
                ClassOptions = _db.LOPs.AsNoTracking()
                    .OrderBy(x => x.TENLOP)
                    .Select(x => new SelectOptionViewModel { Id = x.MALOP, Name = x.TENLOP })
                    .ToList(),
                SubjectOptions = _db.MONHOCs.AsNoTracking()
                    .OrderBy(x => x.TENMONHOC)
                    .Select(x => new SelectOptionViewModel { Id = x.MAMONHOC, Name = x.TENMONHOC })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AdminLessonEditViewModel model)
        {
            ViewBag.AdminNav = "lessons";

            if (!ModelState.IsValid)
            {
                model.ClassOptions = _db.LOPs.AsNoTracking()
                    .OrderBy(x => x.TENLOP)
                    .Select(x => new SelectOptionViewModel { Id = x.MALOP, Name = x.TENLOP })
                    .ToList();
                model.SubjectOptions = _db.MONHOCs.AsNoTracking()
                    .OrderBy(x => x.TENMONHOC)
                    .Select(x => new SelectOptionViewModel { Id = x.MAMONHOC, Name = x.TENMONHOC })
                    .ToList();
                return View(model);
            }

            var lesson = new BAIHOC
            {
                TENBAIHOC = model.LessonName.Trim(),
                MAMONHOC = model.SubjectId,
                MALOP = model.ClassId,
                NOIDUNGLYTUYET = model.TheoryContent,
                MEOGHINHO = model.TipContent,
                VIDU = model.ExampleContent,
                IsDeleted = false
            };

            _db.BAIHOCs.Add(lesson);
            _db.SaveChanges();

            TempData["Message"] = "Tạo bài học thành công.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            ViewBag.AdminNav = "lessons";

            var lesson = _db.BAIHOCs.Find(id);
            if (lesson == null || lesson.IsDeleted)
            {
                return HttpNotFound();
            }

            var model = new AdminLessonEditViewModel
            {
                LessonId = lesson.MABAIHOC,
                LessonName = lesson.TENBAIHOC,
                SubjectId = lesson.MAMONHOC,
                ClassId = lesson.MALOP,
                TheoryContent = lesson.NOIDUNGLYTUYET,
                TipContent = lesson.MEOGHINHO,
                ExampleContent = lesson.VIDU,
                ClassOptions = _db.LOPs.AsNoTracking()
                    .OrderBy(x => x.TENLOP)
                    .Select(x => new SelectOptionViewModel { Id = x.MALOP, Name = x.TENLOP })
                    .ToList(),
                SubjectOptions = _db.MONHOCs.AsNoTracking()
                    .OrderBy(x => x.TENMONHOC)
                    .Select(x => new SelectOptionViewModel { Id = x.MAMONHOC, Name = x.TENMONHOC })
                    .ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AdminLessonEditViewModel model)
        {
            ViewBag.AdminNav = "lessons";

            if (!ModelState.IsValid)
            {
                model.ClassOptions = _db.LOPs.AsNoTracking()
                    .OrderBy(x => x.TENLOP)
                    .Select(x => new SelectOptionViewModel { Id = x.MALOP, Name = x.TENLOP })
                    .ToList();
                model.SubjectOptions = _db.MONHOCs.AsNoTracking()
                    .OrderBy(x => x.TENMONHOC)
                    .Select(x => new SelectOptionViewModel { Id = x.MAMONHOC, Name = x.TENMONHOC })
                    .ToList();
                return View(model);
            }

            var lesson = _db.BAIHOCs.Find(model.LessonId);
            if (lesson == null || lesson.IsDeleted)
            {
                return HttpNotFound();
            }

            lesson.TENBAIHOC = model.LessonName.Trim();
            lesson.MAMONHOC = model.SubjectId;
            lesson.MALOP = model.ClassId;
            lesson.NOIDUNGLYTUYET = model.TheoryContent;
            lesson.MEOGHINHO = model.TipContent;
            lesson.VIDU = model.ExampleContent;

            _db.Entry(lesson).State = EntityState.Modified;
            _db.SaveChanges();

            TempData["Message"] = "Cập nhật bài học thành công.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var lesson = _db.BAIHOCs.Find(id);
            if (lesson == null)
            {
                return HttpNotFound();
            }

            lesson.IsDeleted = true;
            _db.Entry(lesson).State = EntityState.Modified;
            _db.SaveChanges();

            TempData["Message"] = "Đã ẩn bài học thành công.";
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

        private sealed class LessonQuestionCountDto
        {
            public int LessonId { get; set; }
            public int QuestionCount { get; set; }
        }
    }
}
