using System;
using System.Threading.Tasks;
using System.Linq;
using Shouldly;
using Xunit;
using toyiyo.todo.Forecasting;
using toyiyo.todo.Projects;
using toyiyo.todo.Jobs;
using Abp.Runtime.Session;
using toyiyo.todo.MultiTenancy;

namespace toyiyo.todo.Tests.Forecasting
{
    public class ForecastingManager_Tests : todoTestBase
    {
        private readonly IForecastingManager _forecastingManager;
        private readonly IProjectManager _projectManager;

        public ForecastingManager_Tests()
        {
            _forecastingManager = Resolve<IForecastingManager>();
            _projectManager = Resolve<IProjectManager>();
        }

        [Fact]
        public async Task Should_Calculate_Forecast_With_No_Completed_Tasks()
        {
            // Arrange
            var project = await CreateProjectWithJobs(5, 0);

            // Act
            var result = await _forecastingManager.CalculateForecast(project, Job.JobLevel.Task);

            // Assert
            result.ShouldNotBeNull();
            result.ActualProgress.Count.ShouldBe(2); // Initial and current points
            result.ActualProgress.All(p => p.CompletedTasks == 0).ShouldBeTrue();
            result.ForecastProgress.ShouldNotBeEmpty();
        }

        [Fact]
        public async Task Should_Calculate_Forecast_With_Some_Completed_Tasks()
        {
            // Arrange
            var project = await CreateProjectWithJobs(5, 2);

            // Act
            var result = await _forecastingManager.CalculateForecast(project, Job.JobLevel.Task);

            // Assert
            result.ShouldNotBeNull();
            result.ActualProgress.ShouldNotBeEmpty();
            result.ActualProgress.Any(p => p.CompletedTasks == 2).ShouldBeTrue();
            result.ForecastProgress.Last().CompletedTasks.ShouldBe(5);
        }

        [Fact]
        public async Task Should_Get_Historical_Progress()
        {
            // Arrange
            var project = await CreateProjectWithJobs(5, 3);

            // Act
            var progress = await _forecastingManager.GetHistoricalProgress(project.Id, Job.JobLevel.Task);

            // Assert
            progress.ShouldNotBeNull();
            progress.Count.ShouldBeGreaterThan(0);
            progress.Last().CompletedTasks.ShouldBe(3);
            progress.Last().TotalTasks.ShouldBe(5);
        }

        private async Task<Project> CreateProjectWithJobs(int totalJobs, int completedJobs)
        {
            var currentUser = await GetCurrentUserAsync();
            var project = await _projectManager.Create(
                Project.Create("Test Project", currentUser, AbpSession.TenantId.Value)
            );

            for (int i = 0; i < totalJobs; i++)
            {
                var job = await CreateJob(project, $"Job {i}");
                if (i < completedJobs)
                {
                    Job.SetStatus(job, Job.Status.Done, currentUser);
                    await UsingDbContextAsync(async context =>
                    {
                        job.LastModificationTime = DateTime.UtcNow.AddDays(-1);
                        await context.SaveChangesAsync();
                    });
                }
            }

            return project;
        }

        private async Task<Job> CreateJob(Project project, string title)
        {
            var currentUser = await GetCurrentUserAsync();
            return await UsingDbContextAsync(async context =>
            {
                var job = Job.Create(project, title, "Description", currentUser, AbpSession.TenantId.Value);
                context.Jobs.Add(job);
                await context.SaveChangesAsync();
                return job;
            });
        }
    }
}
