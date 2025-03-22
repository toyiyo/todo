using System;

namespace toyiyo.todo.Projects.Dto
{
    /// <summary>
    /// Data transfer object for project progress statistics
    /// </summary>
    public class ProjectProgressDto
    {
        // Base statistics mapped from domain
        public int TotalJobCount { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int BacklogTasks { get; set; }
        public int EpicCount { get; set; }
        public int TaskCount { get; set; }
        public int BugCount { get; set; }
        public DateTime? DueDate { get; set; }
        public int CompletedEpics { get; set; }
        public decimal InProgressPercentage { get; set; }
        public decimal TotalTasksPercentage { get; set; }

        public string Status { get; set; }
        public string StatusClass { get; set; }

        public static ProjectProgressDto FromDomain(ProjectProgress progress)
        {
            if (progress == null) return new ProjectProgressDto();
            
            return new ProjectProgressDto
            {
                TotalJobCount = progress.TotalJobCount,
                CompletedTasks = progress.CompletedTasks,
                InProgressTasks = progress.InProgressTasks,
                BacklogTasks = progress.BacklogTasks,
                EpicCount = progress.EpicCount,
                TaskCount = progress.TaskCount,
                BugCount = progress.BugCount,
                DueDate = progress.DueDate,
                CompletedEpics = progress.CompletedEpics,
                InProgressPercentage = progress.InProgressPercentage,
                TotalTasksPercentage = progress.TotalTasksPercentage,
                Status = progress.HealthStatus?.Status,
                StatusClass = progress.HealthStatus?.CssClass
            };
        }
    }
}
