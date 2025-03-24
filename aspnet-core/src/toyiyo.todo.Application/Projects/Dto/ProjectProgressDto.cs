using System;
using Abp.AutoMapper;

namespace toyiyo.todo.Projects.Dto
{
    [AutoMapFrom(typeof(ProjectProgress))]
    public class ProjectProgressDto
    {
        public int TotalJobCount { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int BacklogTasks { get; set; }
        public int EpicCount { get; set; }
        public int TaskCount { get; set; }
        public int BugCount { get; set; }
        public int CompletedBugs { get; set; }
        public int CompletedEpics { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal InProgressPercentage { get; set; }
        public decimal TotalTasksPercentage { get; set; }
        public decimal EpicCompletionPercentage { get; set; }
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
                CompletedBugs = progress.CompletedBugs,
                CompletedEpics = progress.CompletedEpics,
                DueDate = progress.DueDate,
                InProgressPercentage = progress.InProgressPercentage,
                TotalTasksPercentage = progress.TotalTasksPercentage,
                EpicCompletionPercentage = progress.EpicCompletionPercentage,
                Status = progress.Status,
                StatusClass = progress.StatusClass
            };
        }
    }
}
