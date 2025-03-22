using System.Linq;
using System.Collections.Generic;
using toyiyo.todo.Jobs;
using System;

namespace toyiyo.todo.Projects
{
    public class ProjectProgress
    {
        public int TotalJobCount { get; private set; }
        public int CompletedTasks { get; private set; }
        public int InProgressTasks { get; private set; }
        public int BacklogTasks { get; private set; }
        public int EpicCount { get; private set; }
        public int TaskCount { get; private set; }
        public int BugCount { get; private set; }
        public DateTime? DueDate { get; private set; }
        public int CompletedEpics { get; private set; }
        public decimal TotalTasksPercentage { get; private set; }
        public decimal InProgressPercentage { get; private set; }
        public ProjectHealthStatus HealthStatus { get; private set; }

        private ProjectProgress() { }

        public static ProjectProgress Calculate(Project project)
        {
            if (project?.Jobs == null || !project.Jobs.Any())
                return new ProjectProgress();

            var nonDeletedJobs = project.Jobs.Where(j => !j.IsDeleted);
            var nonEpicJobs = nonDeletedJobs.Where(j => j.Level != Job.JobLevel.Epic);
            var totalNonEpicJobs = nonEpicJobs.Count();

            var progress = new ProjectProgress
            {
                // Only count non-epic jobs for task totals
                TotalJobCount = totalNonEpicJobs,
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
                TotalTasksPercentage = totalNonEpicJobs > 0
                    ? Math.Round((decimal)nonEpicJobs.Count(j => j.JobStatus == Job.Status.Done) / totalNonEpicJobs * 100, 2) 
                    : 0,
                    
                InProgressPercentage = totalNonEpicJobs > 0
                    ? Math.Round((decimal)nonEpicJobs.Count(j => j.JobStatus == Job.Status.InProgress) / totalNonEpicJobs * 100, 2)
                    : 0,

                DueDate = nonDeletedJobs.Max(j => (DateTime?)j.DueDate) ?? null,

                HealthStatus = ProjectHealthStatus.Calculate(
                    totalJobCount: totalNonEpicJobs,
                    completedTasks: nonEpicJobs.Count(j => j.JobStatus == Job.Status.Done),
                    bugCount: nonDeletedJobs.Count(j => j.Level == Job.JobLevel.Bug),
                    dueDate: nonDeletedJobs.Max(j => (DateTime?)j.DueDate)
                )
            };

            return progress;
        }
    }
}
