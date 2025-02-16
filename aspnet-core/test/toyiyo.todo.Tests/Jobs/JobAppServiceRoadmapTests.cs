using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using toyiyo.todo.Jobs;
using toyiyo.todo.Jobs.Dto;
using toyiyo.todo.Projects;
using Xunit;
using static toyiyo.todo.Jobs.Job;

namespace toyiyo.todo.Tests.Jobs
{
    public class JobAppServiceRoadmapTests : todoTestBase
    {
        private readonly IJobAppService _jobAppService;
        private readonly IProjectAppService _projectAppService;

        public JobAppServiceRoadmapTests()
        {
            _jobAppService = Resolve<IJobAppService>();
            _projectAppService = Resolve<IProjectAppService>();
            LoginAsDefaultTenantAdmin();
        }

        [Fact]
        public async Task GetRoadmapView_ShouldReturnJobsWithinDateRange()
        {
            // Arrange
            var projectDto = await _projectAppService.Create(new CreateProjectInputDto { Title = "Roadmap Project" });

            // Create jobs with different date ranges
            var job1 = await _jobAppService.Create(new JobCreateInputDto
            {
                ProjectId = projectDto.Id,
                Title = "Job 1",
                Description = "Job within range",
                DueDate = DateTime.Now.AddDays(10),
                StartDate = DateTime.Now,
                Level = JobLevel.Epic
            });

            var job2 = await _jobAppService.Create(new JobCreateInputDto
            {
                ProjectId = projectDto.Id,
                Title = "Job 2",
                Description = "Job outside range (future)",
                DueDate = DateTime.Now.AddDays(30),
                StartDate = DateTime.Now.AddDays(20),
                Level = JobLevel.Epic
            });

            var job3 = await _jobAppService.Create(new JobCreateInputDto
            {
                ProjectId = projectDto.Id,
                Title = "Job 3",
                Description = "Job outside range (past)",
                DueDate = DateTime.Now.AddDays(30),
                StartDate = DateTime.Now.AddDays(25),
                Level = JobLevel.Epic
            });

            // Define date range
            DateTime startDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now.AddDays(15);

            // Act
            var roadmapView = await _jobAppService.GetRoadmapView(startDate, endDate);

            // Assert
            Assert.NotNull(roadmapView);
            Assert.Equal(startDate, roadmapView.StartDate);
            Assert.Equal(endDate, roadmapView.EndDate);
            Assert.Single(roadmapView.Jobs); // Expect only job1 to be returned
            Assert.Equal(job1.Id, roadmapView.Jobs[0].Id);
        }

        [Fact]
        public async Task GetRoadmapView_ShouldReturnJobsWithNoDatesWhenInRange()
        {
            // Arrange
            var projectDto = await _projectAppService.Create(new CreateProjectInputDto { Title = "Roadmap Project 2" });

            // Create a job with no dates set
            var job1 = await _jobAppService.Create(new JobCreateInputDto
            {
                ProjectId = projectDto.Id,
                Title = "Job 1",
                Description = "Job with no dates",
                Level = JobLevel.Epic
            });

            // Define date range
            DateTime startDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now.AddDays(15);

            // Act
            var roadmapView = await _jobAppService.GetRoadmapView(startDate, endDate);

            // Assert
            Assert.NotNull(roadmapView);
            Assert.Equal(startDate, roadmapView.StartDate);
            Assert.Equal(endDate, roadmapView.EndDate);
            Assert.Single(roadmapView.Jobs); // Expect only job1 to be returned
            Assert.Equal(job1.Id, roadmapView.Jobs[0].Id);
        }

        [Fact]
        public async Task GetRoadmapView_ShouldReturnJobsSpanningDateRange()
        {
            // Arrange
            var projectDto = await _projectAppService.Create(new CreateProjectInputDto { Title = "Roadmap Project 3" });

            // Create a job that spans the date range
            var job1 = await _jobAppService.Create(new JobCreateInputDto
            {
                ProjectId = projectDto.Id,
                Title = "Job 1",
                Description = "Job spanning date range",
                StartDate = DateTime.Now.AddDays(-10),
                DueDate = DateTime.Now.AddDays(20),
                Level = JobLevel.Epic
            });

            // Define date range
            DateTime startDate = DateTime.Now.AddDays(-5);
            DateTime endDate = DateTime.Now.AddDays(15);

            // Act
            var roadmapView = await _jobAppService.GetRoadmapView(startDate, endDate);

            // Assert
            Assert.NotNull(roadmapView);
            Assert.Equal(startDate, roadmapView.StartDate);
            Assert.Equal(endDate, roadmapView.EndDate);
            Assert.Single(roadmapView.Jobs); // Expect only job1 to be returned
            Assert.Equal(job1.Id, roadmapView.Jobs[0].Id);
        }
    }
}
