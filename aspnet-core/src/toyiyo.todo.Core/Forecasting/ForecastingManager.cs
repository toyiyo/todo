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
            var remainingJobs = relevantJobs.Count(j => j.JobStatus != Job.Status.Done);
            var completedJobs = relevantJobs.Where(j => j.JobStatus == Job.Status.Done).ToList();

            // Calculate velocity (tasks/week) based on last 6 weeks
            var sixWeeksAgo = DateTime.UtcNow.AddDays(-HistoricalWeeks * 7);
            var recentCompletionsCount = completedJobs.Count(j => j.LastModificationTime >= sixWeeksAgo);

            var weeklyVelocity = recentCompletionsCount / (decimal)HistoricalWeeks;
            weeklyVelocity = Math.Max(1, weeklyVelocity); // Minimum velocity of 1

            // Calculate completion dates using await Task.Run for CPU-bound work
            var (estimatedDate, optimisticDate, conservativeDate) = await Task.Run(() =>
            {
                var weeksToComplete = remainingJobs / weeklyVelocity;
                return (
                    DateTime.UtcNow.AddDays((double)(weeksToComplete * 7)),
                    DateTime.UtcNow.AddDays((double)(weeksToComplete * 7 * P10_FACTOR)),
                    DateTime.UtcNow.AddDays((double)(weeksToComplete * 7 * P90_FACTOR))
                );
            });

            // Build progress points
            var actualProgress = await Task.Run(() => 
                BuildActualProgressPoints(completedJobs, relevantJobs.Count));
            
            var forecastProgress = await Task.Run(() => 
                BuildForecastProgress(
                    actualProgress[actualProgress.Count - 1].CompletedTasks,
                    relevantJobs.Count,
                    weeklyVelocity,
                    DateTime.UtcNow,
                    estimatedDate
                ));

            var optimisticProgress = await Task.Run(() => 
                BuildForecastProgress(
                    actualProgress[actualProgress.Count - 1].CompletedTasks,
                    relevantJobs.Count,
                    weeklyVelocity / P10_FACTOR,
                    DateTime.UtcNow,
                    optimisticDate
                ));

            var conservativeProgress = await Task.Run(() => 
                BuildForecastProgress(
                    actualProgress[actualProgress.Count - 1].CompletedTasks,
                    relevantJobs.Count,
                    weeklyVelocity / P90_FACTOR,
                    DateTime.UtcNow,
                    conservativeDate
                ));

            return ForecastResult.Create(
                estimatedDate,
                optimisticDate,
                conservativeDate,
                0.8m,
                actualProgress,
                forecastProgress,
                optimisticProgress,
                conservativeProgress
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
            if (completedJobs.Count == 0)
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

            // Add final point at current date if needed
            var lastPoint = progress.Count > 0 ? progress[progress.Count - 1] : null;
            if (lastPoint?.Date < DateTime.UtcNow)
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
            DateTime _ /* endDate parameter kept for compatibility */)
        {
            var progress = new List<ProgressPoint>();
            var twoYearsFromNow = DateTime.UtcNow.AddYears(2);
            var currentDate = startDate;
            var currentCompleted = startingCompleted;
            
            // Add starting point
            progress.Add(ProgressPoint.Create(startDate, currentCompleted, totalJobs));

            // Calculate weekly forecast points until completion or timeout
            while (currentCompleted < totalJobs && currentDate < twoYearsFromNow)
            {
                currentDate = currentDate.AddDays(7);
                currentCompleted = Math.Min(totalJobs, 
                    (int)(startingCompleted + (weeklyVelocity * ((currentDate - startDate).Days / 7))));
                
                progress.Add(ProgressPoint.Create(currentDate, currentCompleted, totalJobs));

                // Safety check - if velocity is too low to make progress
                if (currentCompleted == progress[progress.Count - 2].CompletedTasks)
                {
                    break;
                }
            }

            // Add final 100% completion point if we haven't reached it
            if (currentCompleted < totalJobs)
            {
                var remainingTasks = totalJobs - startingCompleted;
                var weeksToComplete = remainingTasks / weeklyVelocity;
                var completionDate = startDate.AddDays((double)(weeksToComplete * 7));
                
                // Cap the completion date at 2 years
                completionDate = completionDate > twoYearsFromNow ? twoYearsFromNow : completionDate;
                progress.Add(ProgressPoint.Create(completionDate, totalJobs, totalJobs));
            }

            return progress;
        }
    }
}
