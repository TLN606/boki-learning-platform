using System;
using System.Collections.Generic;

namespace Boki_ELearning.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalStudents { get; set; }
        public int TotalLessons { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalExamAttempts { get; set; }
        public int TotalQuizAttempts { get; set; }
        public List<AdminRecentStudentViewModel> RecentStudents { get; set; }
        public List<AdminRecentResultViewModel> RecentResults { get; set; }
        public List<AdminLessonProgressViewModel> LessonProgress { get; set; }
        public List<AdminClassProgressViewModel> ClassProgress { get; set; }
    }

    public class AdminRecentStudentViewModel
    {
        public string FullName { get; set; }
        public string ClassName { get; set; }
        public string Email { get; set; }
    }

    public class AdminRecentResultViewModel
    {
        public string StudentName { get; set; }
        public string LessonName { get; set; }
        public double Score { get; set; }
        public DateTime SubmitAt { get; set; }
    }

    public class AdminStudentListItemViewModel
    {
        public int StudentId { get; set; }
        public string FullName { get; set; }
        public string ClassName { get; set; }
        public string Email { get; set; }
        public int AttemptCount { get; set; }
    }

    public class AdminResultListItemViewModel
    {
        public int ResultId { get; set; }
        public string StudentName { get; set; }
        public string LessonName { get; set; }
        public string SubjectName { get; set; }
        public double Score { get; set; }
        public DateTime SubmitAt { get; set; }
    }

    public class AdminLessonProgressViewModel
    {
        public string LessonName { get; set; }
        public string SubjectName { get; set; }
        public string ClassName { get; set; }
        public int StudentCount { get; set; }
        public int AttemptCount { get; set; }
        public double AvgScore { get; set; }
    }

    public class AdminClassProgressViewModel
    {
        public string ClassName { get; set; }
        public int StudentCount { get; set; }
        public int LessonsCompleted { get; set; }
        public int TotalAttempts { get; set; }
        public double AvgScore { get; set; }
    }
}
