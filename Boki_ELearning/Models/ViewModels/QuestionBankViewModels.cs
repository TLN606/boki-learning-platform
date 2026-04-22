using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Boki_ELearning.Models.ViewModels
{
    public class QuestionBankPageViewModel
    {
        public QuestionBankFilterViewModel Filter { get; set; }
        public QuestionBankEditViewModel Editor { get; set; }
        public List<QuestionBankListItemViewModel> Questions { get; set; }
        public List<QuestionBankLessonOptionViewModel> LessonOptions { get; set; }
    }

    public class QuestionBankFilterViewModel
    {
        public string Keyword { get; set; }
        public int? LessonId { get; set; }
        public string Difficulty { get; set; }
        public string Status { get; set; }
    }

    public class QuestionBankLessonOptionViewModel
    {
        public int LessonId { get; set; }
        public string LessonDisplay { get; set; }
    }

    public class QuestionBankListItemViewModel
    {
        public int QuestionId { get; set; }
        public int LessonId { get; set; }
        public string LessonName { get; set; }
        public string SubjectName { get; set; }
        public string ClassName { get; set; }
        public string QuestionText { get; set; }
        public string Difficulty { get; set; }
        public string Status { get; set; }
        public int AnswerCount { get; set; }
    }

    public class QuestionBankEditViewModel
    {
        public int? QuestionId { get; set; }

        [Required(ErrorMessage = "Vui long chon bai hoc.")]
        public int LessonId { get; set; }

        [Required(ErrorMessage = "Vui long nhap noi dung cau hoi.")]
        [AllowHtml]
        public string QuestionText { get; set; }

        [Required(ErrorMessage = "Vui long chon do kho.")]
        public string Difficulty { get; set; }

        [Required(ErrorMessage = "Vui long chon trang thai.")]
        public string Status { get; set; }

        [Required(ErrorMessage = "Vui long nhap dap an A.")]
        public string AnswerA { get; set; }

        [Required(ErrorMessage = "Vui long nhap dap an B.")]
        public string AnswerB { get; set; }

        [Required(ErrorMessage = "Vui long nhap dap an C.")]
        public string AnswerC { get; set; }

        [Required(ErrorMessage = "Vui long nhap dap an D.")]
        public string AnswerD { get; set; }

        [Range(0, 3, ErrorMessage = "Dap an dung khong hop le.")]
        public int CorrectIndex { get; set; }
    }

    public class QuestionBankImportItemViewModel
    {
        public int LessonId { get; set; }
        public string QuestionText { get; set; }
        public string Difficulty { get; set; }
        public string Status { get; set; }
        public string AnswerA { get; set; }
        public string AnswerB { get; set; }
        public string AnswerC { get; set; }
        public string AnswerD { get; set; }
        public int CorrectIndex { get; set; }
    }
}
