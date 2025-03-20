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
        public decimal CompletedTasksPercentage { get; private set; }
        public decimal InProgressPercentage { get; private set; }
        public decimal TotalTasksPercentage { get; private set; }
        public decimal InProgressTasksPercentage { get; private set; }

        private ProjectProgress() { }

        public static ProjectProgress Calculate(Project project)
        {
            if (project?.Jobs == null || !project.Jobs.Any())
                return new ProjectProgress();

            var nonDeletedJobs = project.Jobs.Where(j => !j.IsDeleted);

            return new ProjectProgress
            {
                TotalTasks = nonDeletedJobs.Count(),
                InProgressTasks = nonDeletedJobs.Count(j => j.JobStatus == Job.Status.InProgress),
                CompletedTasks = nonDeletedJobs.Count(j => j.JobStatus == Job.Status.Done && j.Level != Job.JobLevel.Epic),
                CompletedEpics = nonDeletedJobs.Count(j => j.JobStatus == Job.Status.Done && j.Level == Job.JobLevel.Epic),
                CompletedTasksPercentage = nonDeletedJobs.Count(j => j.JobStatus == Job.Status.Done) > 0 
                    ? (decimal)nonDeletedJobs.Count(j => j.JobStatus == Job.Status.Done) / nonDeletedJobs.Count() * 100 
                    : 0,
                InProgressTasksPercentage = nonDeletedJobs.Count(j => j.JobStatus == Job.Status.InProgress && j.Level != Job.JobLevel.Epic) > 0
                    ? (decimal)nonDeletedJobs.Count(j => j.JobStatus == Job.Status.InProgress) / nonDeletedJobs.Count() * 100 
                    : 0,
                TotalTasksPercentage = nonDeletedJobs.Count() > 0
                    ? (decimal)nonDeletedJobs.Count(j => j.JobStatus == Job.Status.Done) / nonDeletedJobs.Count() * 100 
                    : 0,
               
                BacklogTasks = nonDeletedJobs.Count(j => j.JobStatus == Job.Status.Open),
                EpicCount = nonDeletedJobs.Count(j => j.Level == Job.JobLevel.Epic),
                TaskCount = nonDeletedJobs.Count(j => j.Level == Job.JobLevel.Task),
                BugCount = nonDeletedJobs.Count(j => j.Level == Job.JobLevel.Bug),
                DueDate = nonDeletedJobs.Max(j => j.DueDate)
            };
        }
    }
}
