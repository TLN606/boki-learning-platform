using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Boki_ELearning.Models.ViewModels
{
    public class LessonAdminPageViewModel
    {
        public LessonAdminFilterViewModel Filter { get; set; }
        public List<LessonAdminListItemViewModel> Lessons { get; set; }
        public List<LessonAdminOptionViewModel> ClassOptions { get; set; }
        public List<LessonAdminOptionViewModel> SubjectOptions { get; set; }
    }

    public class LessonAdminFilterViewModel
    {
        public string Keyword { get; set; }
        public int? ClassId { get; set; }
        public int? SubjectId { get; set; }
    }

    public class LessonAdminListItemViewModel
    {
        public int LessonId { get; set; }
        public string LessonName { get; set; }
        public string ClassName { get; set; }
        public string SubjectName { get; set; }
        public string TheoryContent { get; set; }
        public string TipContent { get; set; }
        public int QuestionCount { get; set; }
    }

    public class LessonAdminEditorViewModel
    {
        public int? LessonId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên bài học.")]
        [StringLength(255, ErrorMessage = "Tên bài học không được vượt quá 255 ký tự.")]
        public string LessonName { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn lớp.")]
        public int ClassId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn môn học.")]
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung bài học.")]
        [AllowHtml]
        public string TheoryContent { get; set; }

        [AllowHtml]
        public string TipContent { get; set; }

        public List<LessonAdminOptionViewModel> ClassOptions { get; set; }
        public List<LessonAdminOptionViewModel> SubjectOptions { get; set; }
    }

    public class LessonAdminOptionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
