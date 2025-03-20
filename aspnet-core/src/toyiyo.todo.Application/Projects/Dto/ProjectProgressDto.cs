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
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int BacklogTasks { get; set; }
        public int EpicCount { get; set; }
        public int TaskCount { get; set; }
        public int BugCount { get; set; }
        public DateTime? DueDate { get; set; }
        public int CompletedEpics { get; set; }
        public decimal CompletedTasksPercentage { get; set; }
        public decimal InProgressPercentage { get; set; } 
        public decimal TotalTasksPercentage { get; set; }

        public decimal CompletionPercentage => TotalTasks > 0 ? 
            (decimal)CompletedTasks / TotalTasks * 100 : 0;

        public string Status => TotalTasksPercentage switch {
            100 => "Completed",
            > 65 => "On Track",
            > 35 => "At Risk",
            _ => "Behind"
        };

        public string StatusClass => Status switch {
            "Completed" => "bg-green-100 text-green-800",
            "On Track" => "bg-blue-100 text-blue-800",
            "At Risk" => "bg-yellow-100 text-yellow-800",
            _ => "bg-red-100 text-red-800"
        };
    }
}
