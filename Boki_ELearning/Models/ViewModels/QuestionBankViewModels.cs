using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Boki_ELearning.Models.ViewModels
{
    public class QuestionBankPageViewModel
    {
        public string CurrentSubject { get; set; }
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
        public string Explanation { get; set; }
    }

    public class QuestionBankEditViewModel
    {
        public int? QuestionId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn bài học.")]
        public int LessonId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung câu hỏi.")]
        public string QuestionText { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn độ khó.")]
        public string Difficulty { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn trạng thái.")]
        public string Status { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập đáp án A.")]
        public string AnswerA { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập đáp án B.")]
        public string AnswerB { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập đáp án C.")]
        public string AnswerC { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập đáp án D.")]
        public string AnswerD { get; set; }

        [Range(0, 3, ErrorMessage = "Đáp án đúng không hợp lệ.")]
        public int CorrectIndex { get; set; }

        public string Explanation { get; set; }
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
        public string Explanation { get; set; }
    }
}
