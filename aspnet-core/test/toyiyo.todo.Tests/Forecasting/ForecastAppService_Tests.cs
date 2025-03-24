using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;
using toyiyo.todo.Forecasting;
using toyiyo.todo.Projects;
using toyiyo.todo.Jobs;
using toyiyo.todo.Projects.Dto;
using toyiyo.todo.Jobs.Dto;

namespace toyiyo.todo.Tests.Forecasting
{
    public class ForecastAppService_Tests : todoTestBase
    {
        private readonly IForecastAppService _forecastAppService;
        private readonly IProjectAppService _projectAppService;

        public ForecastAppService_Tests()
        {
            _forecastAppService = Resolve<IForecastAppService>();
            _projectAppService = Resolve<IProjectAppService>();
        }

        [Fact]
        public async Task Should_Get_Forecast_For_Project()
        {
            // Arrange
            var project = await CreateTestProjectWithJobs();

            // Act
            var result = await _forecastAppService.GetForecast(project.Id, Job.JobLevel.Task);

            // Assert
            result.ShouldNotBeNull();
            result.EstimatedCompletionDate.ShouldBeGreaterThan(DateTime.UtcNow);
            result.OptimisticCompletionDate.ShouldBeLessThanOrEqualTo(result.EstimatedCompletionDate);
            result.ConservativeCompletionDate.ShouldBeGreaterThanOrEqualTo(result.EstimatedCompletionDate);
            result.ActualProgress.ShouldNotBeEmpty();
            result.ForecastProgress.ShouldNotBeEmpty();
        }

        [Theory]
        [InlineData(Job.JobLevel.Task)]
        [InlineData(Job.JobLevel.Epic)]
        public async Task Should_Handle_Different_Job_Levels(Job.JobLevel level)
        {
            // Arrange
            var project = await CreateTestProjectWithJobs(level);

            // Act
            var result = await _forecastAppService.GetForecast(project.Id, level);

            // Assert
            result.ShouldNotBeNull();
            result.ActualProgress.ShouldNotBeNull();
            result.ForecastProgress.ShouldNotBeNull();
        }

        private async Task<ProjectDto> CreateTestProjectWithJobs(Job.JobLevel level = Job.JobLevel.Task)
        {
            var project = await _projectAppService.Create(new CreateProjectInputDto 
            { 
                Title = "Test Project" 
            });

            // Create a mix of open and completed jobs
            for (int i = 0; i < 5; i++)
            {
                var job = await CreateJob(project.Id, $"Job {i}", level);
                if (i < 2)
                {
                    await SetJobStatus(job.Id, Job.Status.Done);
                }
            }

            return project;
        }

        private async Task<JobDto> CreateJob(Guid projectId, string title, Job.JobLevel level)
        {
            return await Resolve<IJobAppService>().Create(new JobCreateInputDto
            {
                ProjectId = projectId,
                Title = title,
                Level = level
            });
        }

        private async Task SetJobStatus(Guid jobId, Job.Status status)
        {
            await Resolve<IJobAppService>().SetJobStatus(new JobSetStatusInputDto
            {
                Id = jobId,
                JobStatus = status
            });
        }
    }
}
