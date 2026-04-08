using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Boki_ELearning.Models.ViewModels
{
    public class AdminExamListItemViewModel
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public int QuestionCount { get; set; }
        public int DurationMinutes { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public int AssignedStudentCount { get; set; }
    }

    public class AdminExamCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đề.")]
        public string ExamName { get; set; }

        [Range(1, 200, ErrorMessage = "Số câu phải từ 1 đến 200.")]
        public int QuestionCount { get; set; }

        [Range(1, 240, ErrorMessage = "Thời gian phải từ 1 đến 240 phút.")]
        public int DurationMinutes { get; set; }

        public DateTime? OpenAt { get; set; }
        public DateTime? CloseAt { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ít nhất một môn.")]
        public List<int> SubjectIds { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ít nhất một lớp.")]
        public List<int> ClassIds { get; set; }
    }

    public class AdminExamCreatePageViewModel
    {
        public AdminExamCreateViewModel Form { get; set; }
        public List<SimpleOptionViewModel> SubjectOptions { get; set; }
        public List<SimpleOptionViewModel> ClassOptions { get; set; }
        public int AvailableQuestionCount { get; set; }
    }

    public class SimpleOptionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class AdminExamDetailViewModel
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public int QuestionCount { get; set; }
        public int DurationMinutes { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? OpenAt { get; set; }
        public DateTime? CloseAt { get; set; }
        public List<string> Subjects { get; set; }
        public List<string> Classes { get; set; }
        public List<AdminExamStudentResultViewModel> StudentResults { get; set; }
    }

    public class AdminExamStudentResultViewModel
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public string ClassName { get; set; }
        public int AttemptCount { get; set; }
        public double? BestScore { get; set; }
        public DateTime? LatestAttemptAt { get; set; }
    }

    public class StudentExamListItemViewModel
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public int QuestionCount { get; set; }
        public int DurationMinutes { get; set; }
        public string Status { get; set; }
        public bool IsPractice { get; set; }
        public double? BestScore { get; set; }
        public int AttemptCount { get; set; }
        public DateTime? LatestAttemptAt { get; set; }
    }

    public class StudentExamTakeViewModel
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public int DurationMinutes { get; set; }
        public List<StudentExamQuestionViewModel> Questions { get; set; }
    }

    public class StudentExamQuestionViewModel
    {
        public int QuestionId { get; set; }
        public int Order { get; set; }
        public string QuestionText { get; set; }
        public List<StudentExamAnswerViewModel> Answers { get; set; }
    }

    public class StudentExamAnswerViewModel
    {
        public int AnswerId { get; set; }
        public string AnswerText { get; set; }
    }

    public class StudentExamSubmitViewModel
    {
        public int ExamId { get; set; }
        public List<StudentExamSubmitAnswerViewModel> Answers { get; set; }
    }

    public class StudentExamSubmitAnswerViewModel
    {
        public int QuestionId { get; set; }
        public int? AnswerId { get; set; }
    }

    public class StudentExamHistoryViewModel
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public double? BestScore { get; set; }
        public List<StudentExamAttemptViewModel> Attempts { get; set; }
    }

    public class StudentExamAttemptViewModel
    {
        public int AttemptId { get; set; }
        public double Score { get; set; }
        public int CorrectCount { get; set; }
        public int TotalCount { get; set; }
        public DateTime SubmitAt { get; set; }
    }

    public class StudentPracticeCreateViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn lớp.")]
        public int? ClassId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn môn học.")]
        public int? SubjectId { get; set; }

        [Range(1, 200, ErrorMessage = "Số câu phải từ 1 đến 200.")]
        public int QuestionCount { get; set; }

        [Range(1, 240, ErrorMessage = "Thời gian phải từ 1 đến 240 phút.")]
        public int DurationMinutes { get; set; }
    }

    public class StudentPracticeCreatePageViewModel
    {
        public StudentPracticeCreateViewModel Form { get; set; }
        public List<SimpleOptionViewModel> SubjectOptions { get; set; }
        public List<SimpleOptionViewModel> ClassOptions { get; set; }
        public int AvailableQuestionCount { get; set; }
    }
}
