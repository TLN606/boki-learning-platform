using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Boki_ELearning.Models.ViewModels
{
    public class LessonDetailViewModel
    {
        public int LessonId { get; set; }
        public string LessonName { get; set; }
        public string SubjectName { get; set; }
        public string ClassName { get; set; }
        public string TheoryContent { get; set; }
        public string TipContent { get; set; }
        public string ExampleContent { get; set; }
        public int QuestionCount { get; set; }
    }

    public class LessonQuizViewModel
    {
        public int LessonId { get; set; }
        public string LessonName { get; set; }
        public int QuestionCount { get; set; }
        public List<LessonQuizQuestionViewModel> Questions { get; set; }
    }

    public class LessonQuizQuestionViewModel
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public List<LessonQuizAnswerViewModel> Answers { get; set; }
    }

    public class LessonQuizAnswerViewModel
    {
        public int AnswerId { get; set; }
        public string AnswerText { get; set; }
    }

    public class LessonQuizSubmitViewModel
    {
        public int LessonId { get; set; }
        public List<LessonQuizSubmitAnswerViewModel> Answers { get; set; }
    }

    public class LessonQuizSubmitAnswerViewModel
    {
        public int QuestionId { get; set; }
        public int? AnswerId { get; set; }
    }

    public class LessonResultQuestionViewModel
    {
        public string QuestionText { get; set; }
        public string SelectedAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public string CorrectAnswer { get; set; }
        public string Explanation { get; set; }
    }

    public class LessonResultViewModel
    {
        public int ResultId { get; set; }
        public string LessonName { get; set; }
        public DateTime SubmitAt { get; set; }
        public int CorrectCount { get; set; }
        public int TotalCount { get; set; }
        public double Score { get; set; }
        public string LessonTheory { get; set; }
        public string LessonTip { get; set; }
        public List<LessonResultQuestionViewModel> Questions { get; set; }
    }
}
