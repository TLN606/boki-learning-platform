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
        public int TotalLearningResults { get; set; }
        public double AverageLearningScore { get; set; }
        public double HighestLearningScore { get; set; }
        public List<AdminRecentStudentViewModel> RecentStudents { get; set; }
        public List<AdminRecentResultViewModel> RecentResults { get; set; }
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
        public string ClassName { get; set; }
        public string LessonName { get; set; }
        public string SubjectName { get; set; }
        public double Score { get; set; }
        public DateTime SubmitAt { get; set; }
    }

    public class AdminResultFilterViewModel
    {
        public string Keyword { get; set; }
        public int? ClassId { get; set; }
        public int? SubjectId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public double? MinScore { get; set; }
        public double? MaxScore { get; set; }
    }

    public class AdminOptionViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class AdminResultStatsViewModel
    {
        public int TotalResults { get; set; }
        public int TotalStudents { get; set; }
        public double AverageScore { get; set; }
        public double HighestScore { get; set; }
        public double LowestScore { get; set; }
        public int PassCount { get; set; }
        public int FailCount { get; set; }
        public double PassRate { get; set; }
    }

    public class AdminResultsPageViewModel
    {
        public AdminResultFilterViewModel Filter { get; set; }
        public List<AdminOptionViewModel> ClassOptions { get; set; }
        public List<AdminOptionViewModel> SubjectOptions { get; set; }
        public AdminResultStatsViewModel Stats { get; set; }
        public List<AdminResultListItemViewModel> Results { get; set; }
    }
}
