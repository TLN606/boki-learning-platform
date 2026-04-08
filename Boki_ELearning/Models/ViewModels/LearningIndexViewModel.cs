using System.Collections.Generic;

namespace Boki_ELearning.Models.ViewModels
{
    public class LearningIndexViewModel
    {
        public string StudentName { get; set; }
        public string ClassName { get; set; }
        public int? SuggestedClassId { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
        public int LessonsWithQuiz { get; set; }
        public int LessonsWithoutQuiz { get; set; }
        public int? SelectedClassId { get; set; }
        public int? SelectedSubjectId { get; set; }
        public bool? SelectedHasQuiz { get; set; }
        public List<LearningFilterOptionViewModel> ClassOptions { get; set; }
        public List<LearningFilterOptionViewModel> SubjectOptions { get; set; }
        public List<SubjectProgressViewModel> SubjectProgress { get; set; }
        public List<LessonItemViewModel> Lessons { get; set; }
    }

    public class LearningFilterOptionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class SubjectProgressViewModel
    {
        public string SubjectName { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }
    }

    public class LessonItemViewModel
    {
        public int LessonId { get; set; }
        public string LessonName { get; set; }
        public string ClassName { get; set; }
        public string SubjectName { get; set; }
        public int QuestionCount { get; set; }
        public bool IsCompleted { get; set; }
    }
}
