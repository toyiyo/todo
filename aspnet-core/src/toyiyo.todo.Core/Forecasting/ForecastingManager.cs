using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Services;
using toyiyo.todo.Projects;
using toyiyo.todo.Jobs;
using Abp.Domain.Repositories;

namespace toyiyo.todo.Forecasting
{
    public class ForecastingManager : DomainService, IForecastingManager
    {
        private const int HistoricalWeeks = 6;
        private const decimal P10_FACTOR = 0.8m;
        private const decimal P90_FACTOR = 1.4m;

        private readonly IRepository<Job, Guid> _jobRepository;

        public ForecastingManager(IRepository<Job, Guid> jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task<ForecastResult> CalculateForecast(Project project, Job.JobLevel level)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));

            var relevantJobs = project.Jobs.Where(j => !j.IsDeleted && j.Level == level).ToList();
            var completedJobs = relevantJobs.Where(j => j.JobStatus == Job.Status.Done).ToList();
            var remainingJobs = relevantJobs.Where(j => j.JobStatus != Job.Status.Done).Count();

            // Calculate velocity (tasks/week) based on last 6 weeks
            var recentCompletions = completedJobs
                .Where(j => j.LastModificationTime >= DateTime.UtcNow.AddDays(-HistoricalWeeks * 7))
                .ToList();

            var weeklyVelocity = recentCompletions.Count / (decimal)HistoricalWeeks;
            if (weeklyVelocity == 0) weeklyVelocity = 1; // Minimum velocity

            // Calculate estimated completion dates
            var weeksToComplete = remainingJobs / weeklyVelocity;
            var estimatedDate = DateTime.UtcNow.AddDays((double)(weeksToComplete * 7));
            var optimisticDate = DateTime.UtcNow.AddDays((double)(weeksToComplete * 7 * P10_FACTOR));
            var conservativeDate = DateTime.UtcNow.AddDays((double)(weeksToComplete * 7 * P90_FACTOR));

            // Build progress points
            var actualProgress = BuildActualProgressPoints(completedJobs, relevantJobs.Count);
            var forecastProgress = BuildForecastProgress(
                actualProgress.Last().CompletedTasks,
                relevantJobs.Count,
                weeklyVelocity,
                DateTime.UtcNow,
                estimatedDate);

            return ForecastResult.Create(
                estimatedDate,
                optimisticDate,
                conservativeDate,
                0.8m, // 80% confidence
                actualProgress,
                forecastProgress
            );
        }

        public async Task<List<ProgressPoint>> GetHistoricalProgress(Guid projectId, Job.JobLevel level)
        {
            if (projectId == Guid.Empty) throw new ArgumentException("Project ID cannot be empty", nameof(projectId));

            var jobs = await _jobRepository.GetAllListAsync(j => 
                j.Project.Id == projectId && 
                j.Level == level &&
                !j.IsDeleted);

            return BuildActualProgressPoints(
                jobs.Where(j => j.JobStatus == Job.Status.Done).ToList(),
                jobs.Count);
        }

        private static List<ProgressPoint> BuildActualProgressPoints(List<Job> completedJobs, int totalJobs)
        {
            var progress = new List<ProgressPoint>();
            
            // No completed jobs case
            if (!completedJobs.Any())
            {
                progress.Add(ProgressPoint.Create(DateTime.UtcNow.AddDays(-HistoricalWeeks * 7), 0, totalJobs));
                progress.Add(ProgressPoint.Create(DateTime.UtcNow, 0, totalJobs));
                return progress;
            }

            var startDate = completedJobs.Min(j => j.LastModificationTime);
            var sixWeeksAgo = DateTime.UtcNow.AddDays(-HistoricalWeeks * 7);
            startDate = startDate > sixWeeksAgo ? startDate : sixWeeksAgo;

            // Create weekly points
            var currentDate = startDate;
            while (currentDate <= DateTime.UtcNow)
            {
                var completedAtPoint = completedJobs.Count(j => j.LastModificationTime <= currentDate);
                progress.Add(ProgressPoint.Create(currentDate.Value, completedAtPoint, totalJobs));
                currentDate = currentDate.Value.AddDays(7);
            }

            // Add final point at current date
            var lastPoint = progress.LastOrDefault();
            if (lastPoint == null || lastPoint.Date < DateTime.UtcNow)
            {
                progress.Add(ProgressPoint.Create(DateTime.UtcNow, completedJobs.Count, totalJobs));
            }

            return progress;
        }

        private static List<ProgressPoint> BuildForecastProgress(
            int startingCompleted,
            int totalJobs,
            decimal weeklyVelocity,
            DateTime startDate,
            DateTime endDate)
        {
            var progress = new List<ProgressPoint>();
            
            // Add starting point
            progress.Add(ProgressPoint.Create(startDate, startingCompleted, totalJobs));

            // Calculate weekly forecast points
            var currentDate = startDate.AddDays(7);
            
            while (currentDate <= endDate)
            {
                var completedTasks = Math.Min(totalJobs, 
                    (int)(startingCompleted + (weeklyVelocity * ((currentDate - startDate).Days / 7))));
                
                progress.Add(ProgressPoint.Create(currentDate, completedTasks, totalJobs));
                currentDate = currentDate.AddDays(7);
            }

            // Add final point if not at end date
            if (currentDate.AddDays(-7) < endDate)
            {
                var finalCompleted = Math.Min(totalJobs, 
                    (int)(startingCompleted + (weeklyVelocity * ((endDate - startDate).Days / 7))));
                
                progress.Add(ProgressPoint.Create(endDate, finalCompleted, totalJobs));
            }

            return progress;
        }
    }
}
