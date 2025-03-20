using System;
using Abp.AutoMapper;

namespace toyiyo.todo.Projects.Dto
{
    /// <summary>
    /// Data transfer object for project progress statistics
    /// </summary>
    [AutoMap(typeof(ProjectProgress))]
    public class ProjectProgressDto
    {
        // Base statistics mapped from domain
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int BacklogTasks { get; set; }
        public int EpicCount { get; set; }
        public int TaskCount { get; set; }
        public int BugCount { get; set; }
        public DateTime? DueDate { get; set; }
        public int CompletedEpics { get; set; }
        
        // Percentages mapped from domain
        public decimal CompletedTasksPercentage { get; set; }
        public decimal InProgressPercentage { get; set; }
        public decimal TotalTasksPercentage { get; set; }

        // Presentation-specific computed properties
        public string Status => TotalTasksPercentage switch {
            100 => "Completed",
            > 65 => "On Track", 
            > 35 => "At Risk",
            _ => "Behind"
        };

        public string StatusClass => Status switch {
            "Completed" => "badge-success",
            "On Track" => "badge-info", 
            "At Risk" => "badge-warning",
            _ => "badge-danger"
        };
    }
}
