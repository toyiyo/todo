using System.Linq;
using System.Collections.Generic;
using toyiyo.todo.Jobs;
using System;

namespace toyiyo.todo.Projects
{
    public class ProjectProgress
    {
        public int TotalTasks { get; private set; }
        public int CompletedTasks { get; private set; }
        public int InProgressTasks { get; private set; }
        public int BacklogTasks { get; private set; }
        public int EpicCount { get; private set; }
        public int TaskCount { get; private set; }
        public int BugCount { get; private set; }
        public DateTime? DueDate { get; private set; }
        public int CompletedEpics { get; private set; }
        public decimal TotalTasksPercentage { get; private set; }

        private ProjectProgress() { }

        public static ProjectProgress Calculate(Project project)
        {
            if (project?.Jobs == null || !project.Jobs.Any())
                return new ProjectProgress();

            var nonDeletedJobs = project.Jobs.Where(j => !j.IsDeleted);
            var nonEpicJobs = nonDeletedJobs.Where(j => j.Level != Job.JobLevel.Epic);

            return new ProjectProgress
            {
                // Only count non-epic jobs for task totals
                TotalTasks = nonEpicJobs.Count(),
                CompletedTasks = nonEpicJobs.Count(j => j.JobStatus == Job.Status.Done),
                InProgressTasks = nonEpicJobs.Count(j => j.JobStatus == Job.Status.InProgress),
                BacklogTasks = nonEpicJobs.Count(j => j.JobStatus == Job.Status.Open),
                
                // Epic counts are separate
                EpicCount = nonDeletedJobs.Count(j => j.Level == Job.JobLevel.Epic),
                CompletedEpics = nonDeletedJobs.Count(j => j.Level == Job.JobLevel.Epic && j.JobStatus == Job.Status.Done),
                
                // Specific job type counts
                TaskCount = nonDeletedJobs.Count(j => j.Level == Job.JobLevel.Task),
                BugCount = nonDeletedJobs.Count(j => j.Level == Job.JobLevel.Bug),
                
                // Calculate percentages based on non-epic jobs only
                TotalTasksPercentage = nonEpicJobs.Any() 
                    ? (decimal)nonEpicJobs.Count(j => j.JobStatus == Job.Status.Done) / nonEpicJobs.Count() * 100 
                    : 0,
                
                DueDate = nonDeletedJobs.Max(j => (DateTime?)j.DueDate) ?? null
            };
        }
    }
}
