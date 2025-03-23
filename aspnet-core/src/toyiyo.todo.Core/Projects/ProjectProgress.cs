using System;
using System.Linq;
using toyiyo.todo.Jobs;
using Abp.Domain.Values;
using System.Collections.Generic;

namespace toyiyo.todo.Projects
{
    public class ProjectProgress : ValueObject
    {
        public decimal TotalTasksPercentage { get; private set; }
        public decimal EpicCompletionPercentage { get; private set; }
        public int TotalJobCount { get; private set; }
        public int CompletedTasks { get; private set; }
        public int TaskCount { get; private set; }
        public int EpicCount { get; private set; }
        public int CompletedEpics { get; private set; }
        public int BugCount { get; private set; }
        public int CompletedBugs { get; private set; }
        public string Status { get; private set; }
        public string StatusClass { get; private set; }
        public int BacklogTasks { get; private set; }
        public int InProgressTasks { get; private set; }
        public decimal InProgressPercentage { get; private set; }
        public DateTime? DueDate { get; private set; }
        public ProjectHealthStatus HealthStatus => 
            ProjectHealthStatus.Calculate(TotalJobCount, CompletedTasks, BugCount, DueDate);

        private ProjectProgress() { }

        public static ProjectProgress Calculate(Project project)
        {
            // Only consider non-deleted jobs
            var nonDeletedJobs = project.Jobs.Where(j => !j.IsDeleted);
            
            // Get work items (tasks & bugs)
            var workItems = nonDeletedJobs.Where(j => 
                j.Level == Job.JobLevel.Task || 
                j.Level == Job.JobLevel.Bug);

            // Calculate epic progress separately
            var epics = nonDeletedJobs.Where(j => j.Level == Job.JobLevel.Epic);

            var epicCount = epics.Count();
            var completedEpics = epics.Count(j => j.JobStatus == Job.Status.Done);
            var epicPercentage = epicCount > 0 
                ? Math.Round((decimal)completedEpics / epicCount * 100, 2) 
                : 0;

            // Calculate work item progress
            var totalWorkItems = workItems.Count();
            var completedWorkItems = workItems.Count(j => j.JobStatus == Job.Status.Done);
            var workItemPercentage = totalWorkItems > 0
                ? Math.Round((decimal)completedWorkItems / totalWorkItems * 100, 2)
                : 0;

            // Count bugs separately
            var bugs = nonDeletedJobs.Where(j => j.Level == Job.JobLevel.Bug);
            var bugCount = bugs.Count();
            var completedBugs = bugs.Count(j => j.JobStatus == Job.Status.Done);

            // Count backlog tasks (open tasks and bugs)
            var backlogTasks = workItems.Count(j => j.JobStatus == Job.Status.Open);

            // Calculate in-progress metrics
            var inProgressTasks = workItems.Count(j => j.JobStatus == Job.Status.InProgress);
            var inProgressPercentage = totalWorkItems > 0 
                ? Math.Round((decimal)inProgressTasks / totalWorkItems * 100, 2)
                : 0;

            // Get latest due date from work items
            var dueDate = workItems
                .Where(j => j.DueDate != default)
                .OrderByDescending(j => j.DueDate)
                .FirstOrDefault()?.DueDate;

            // Determine overall status
            var (status, statusClass) = DetermineStatus(workItemPercentage, bugCount, completedBugs);

            return new ProjectProgress
            {
                TotalTasksPercentage = workItemPercentage,
                EpicCompletionPercentage = epicPercentage,
                TotalJobCount = nonDeletedJobs.Count(),
                CompletedTasks = completedWorkItems,
                TaskCount = workItems.Count(),
                EpicCount = epicCount,
                CompletedEpics = completedEpics,
                BugCount = bugCount,
                CompletedBugs = completedBugs,
                BacklogTasks = backlogTasks,
                InProgressTasks = inProgressTasks,
                InProgressPercentage = inProgressPercentage,
                DueDate = dueDate,
                Status = status,
                StatusClass = statusClass
            };
        }

        private static (string status, string statusClass) DetermineStatus(
            decimal completionPercentage, int bugCount, int completedBugs)
        {
            // High risk if there are many open bugs
            if (bugCount > completedBugs * 2)
            {
                return ("At Risk", "badge-danger");
            }
            
            // Status based on completion percentage
            if (completionPercentage == 100)
            {
                return ("Complete", "badge-success");
            }
            if (completionPercentage == 0)
            {
                return ("Not Started", "badge-secondary");
            }
            if (completionPercentage >= 80)
            {
                return ("Nearly Done", "badge-info");
            }
            
            return ("In Progress", "badge-primary");
        }

        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return TotalTasksPercentage;
            yield return EpicCompletionPercentage;
            yield return TotalJobCount;
            yield return CompletedTasks;
            yield return TaskCount;
            yield return EpicCount;
            yield return CompletedEpics;
            yield return BugCount;
            yield return CompletedBugs;
            yield return BacklogTasks;
            yield return Status;
            yield return StatusClass;
        }
    }
}
