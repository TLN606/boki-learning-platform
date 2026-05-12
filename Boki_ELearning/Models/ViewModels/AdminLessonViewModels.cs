using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Boki_ELearning.Models.ViewModels
{
    public class AdminLessonListViewModel
    {
        public List<AdminLessonListItemViewModel> Lessons { get; set; }
        public int? FilterClassId { get; set; }
        public int? FilterSubjectId { get; set; }
        public List<SelectOptionViewModel> ClassOptions { get; set; }
        public List<SelectOptionViewModel> SubjectOptions { get; set; }
    }

    public class AdminLessonListItemViewModel
    {
        public int LessonId { get; set; }
        public string LessonName { get; set; }
        public string SubjectName { get; set; }
        public string ClassName { get; set; }
        public int QuestionCount { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class AdminLessonEditViewModel
    {
        public int? LessonId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên bài học.")]
        [StringLength(255)]
        public string LessonName { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn môn học.")]
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn lớp.")]
        public int ClassId { get; set; }

        public string TheoryContent { get; set; }
        public string TipContent { get; set; }
        public string ExampleContent { get; set; }

        public List<SelectOptionViewModel> ClassOptions { get; set; }
        public List<SelectOptionViewModel> SubjectOptions { get; set; }
    }
}
