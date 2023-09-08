using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using toyiyo.todo.Projects;
using toyiyo.todo.Sessions;
using toyiyo.todo.Jobs;
using Xunit;
using static toyiyo.todo.Jobs.Job;
using System;
using Microsoft.AspNetCore.Mvc;

namespace toyiyo.todo.Tests.Jobs
{
    public class JobAppServiceTests : todoTestBase
    {
        private readonly IProjectAppService _projectAppService;
        private readonly ISessionAppService _sessionAppService;
        private readonly IJobAppService _jobAppService;
        public JobAppServiceTests()
        {
            _projectAppService = Resolve<IProjectAppService>();
            _sessionAppService = Resolve<ISessionAppService>();
            _jobAppService = Resolve<IJobAppService>();
        }
        [Fact]
        public async Task CreateJob_ReturnsNewJob()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = await _projectAppService.Create(new CreateProjectInputDto() { Title = "test" });

            // Act
            var job = await _jobAppService.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job", Description = "test job" });

            // Assert
            job.ShouldNotBeNull();
            job.Title.ShouldBe("test job");
            job.Description.ShouldBe("test job");
            job.JobStatus.ShouldBe(Status.Open);
            job.Assignee.Id.ShouldBe(currentUser.Id);
            job.Owner.Id.ShouldBe(currentUser.Id);
            
        }

        [Fact]
        public async Task CreateJob_DueDateIsToday_ReturnsNewJob()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = await _projectAppService.Create(new CreateProjectInputDto() { Title = "test" });
            var dueDate = DateTime.UtcNow;

            // Act
            var job = await _jobAppService.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job", Description = "test job", DueDate = dueDate });

            // Assert
            job.ShouldNotBeNull();
            job.Title.ShouldBe("test job");
            job.Description.ShouldBe("test job");
            job.JobStatus.ShouldBe(Status.Open);
            job.Assignee.Id.ShouldBe(currentUser.Id);
            job.Owner.Id.ShouldBe(currentUser.Id);
            job.DueDate.ShouldBe(dueDate);
        }

        [Fact]
        public async Task CreateJob_DueDateIsYesterday_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = await _projectAppService.Create(new CreateProjectInputDto() { Title = "test" });
            var dueDate = DateTime.UtcNow.AddDays(-1);

            // Act
            await Assert.ThrowsAnyAsync<Exception>(async () => await _jobAppService.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job", Description = "test job", DueDate = dueDate }));
        }
        [Fact]
        public async Task CreateJob_WithParentId_CreatesSubtask()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = await _projectAppService.Create(new CreateProjectInputDto() { Title = "test" });
            var parentJob = await _jobAppService.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "parent job", Description = "parent job" });

            // Act
            var subtask = await _jobAppService.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "subtask", Description = "subtask", ParentId = parentJob.Id, Level =  JobLevel.SubTask });

            // Assert
            subtask.ShouldNotBeNull();
            subtask.Title.ShouldBe("subtask");
            subtask.Description.ShouldBe("subtask");
            subtask.ParentId.ShouldBe(parentJob.Id);
            subtask.Level.ShouldBe(JobLevel.SubTask);
        }

        [Fact]
        public async Task GetJob_ReturnsJob()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = await _projectAppService.Create(new CreateProjectInputDto() { Title = "test" });
            var job = await _jobAppService.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job", Description = "test job" });

            // Act
            var job2 = await _jobAppService.Get(job.Id);

            // Assert
            job2.ShouldNotBeNull();
            job2.Title.ShouldBe("test job");
        }

        [Fact]
        public async Task GetAllJobs_FilterByJobLevel() {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = await _projectAppService.Create(new CreateProjectInputDto(){ Title = "test"});
            var job = await _jobAppService.Create(new JobCreateInputDto(){ ProjectId = project.Id, Title = "test job", Description = "test job"});
            var subtask = await _jobAppService.Create(new JobCreateInputDto(){ ProjectId = project.Id, Title = "subtask", Description = "subtask", ParentId = job.Id, Level =  JobLevel.SubTask});

            // Act
            var jobs = await _jobAppService.GetAll(new GetAllJobsInput(){ ProjectId = project.Id, Level = JobLevel.Task});
            var subtasks = await _jobAppService.GetAll(new GetAllJobsInput(){ ProjectId = project.Id, Level = JobLevel.SubTask});
            var all = await _jobAppService.GetAll(new GetAllJobsInput(){ ProjectId = project.Id});

            // Assert
            jobs.Items.Count.ShouldBe(1);
            jobs.Items[0].Id.ShouldBe(job.Id);

            subtasks.Items.Count.ShouldBe(1);
            subtasks.Items[0].Id.ShouldBe(subtask.Id);

            all.Items.Count.ShouldBe(2);
            all.Items[0].Id.ShouldBe(job.Id);
            all.Items[1].Id.ShouldBe(subtask.Id);
        }
        [Fact]
        public async Task GetAllJobs_FilterByJobStatus()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = await _projectAppService.Create(new CreateProjectInputDto() { Title = "test" });
            var job = await _jobAppService.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job", Description = "test job" });
            await _jobAppService.SetJobStatus(new JobSetStatusInputDto() { Id = job.Id, JobStatus = Status.Done });

            // Act
            var openJobs = await _jobAppService.GetAll(new GetAllJobsInput() { JobStatus = Status.Open });
            var doneJobs = await _jobAppService.GetAll(new GetAllJobsInput() { JobStatus = Status.Done });

            // Assert
            doneJobs.Items.Count.ShouldBe(1);
            doneJobs.Items[0].Id.ShouldBe(job.Id);
            openJobs.Items.Count.ShouldBe(0);
        }

        [Fact]
        public async Task GetAllJobs_ReturnsJobsSortedByCreateDateDesc()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = await _projectAppService.Create(new CreateProjectInputDto() { Title = "test" });
            var job = await _jobAppService.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job", Description = "test job" });
            var job2 = await _jobAppService.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job2", Description = "test job2" });

            // Act
            var jobs = await _jobAppService.GetAll(new GetAllJobsInput() { ProjectId = project.Id });

            // Assert
            jobs.ShouldNotBeNull();
            jobs.Items.Count.ShouldBe(2);
            jobs.Items.First().Title.ShouldBe("test job2");
            jobs.Items[1].Title.ShouldBe("test job");
        }
        [Fact]
        public async Task GetAllJobs_ReturnsJobsSortedByCreateDateAsc()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = await _projectAppService.Create(new CreateProjectInputDto() { Title = "test" });
            var job = await _jobAppService.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job", Description = "test job" });
            var job2 = await _jobAppService.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job2", Description = "test job2" });

            // Act
            var jobs = await _jobAppService.GetAll(new GetAllJobsInput() { ProjectId = project.Id, Sorting = "CreationTime ASC" });

            // Assert
            jobs.ShouldNotBeNull();
            jobs.Items.Count.ShouldBe(2);
            jobs.Items.First().Title.ShouldBe("test job");
            jobs.Items[1].Title.ShouldBe("test job2");
            jobs.Items.First().Assignee?.Id.ShouldBe(currentUser.Id);
            jobs.Items.First().Owner?.Id.ShouldBe(currentUser.Id);
            jobs.Items.First().JobStatus.ShouldBe(Status.Open);
            jobs.Items.First().Project?.Id.ShouldBe(project.Id);
        }
        [Fact]
        public async Task GetAllJobs_ReturnsJobsSortedByTitleAsc()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = await _projectAppService.Create(new CreateProjectInputDto() { Title = "test" });
            var job = await _jobAppService.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "A", Description = "test job" });
            var job2 = await _jobAppService.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "Z", Description = "test job2" });

            // Act
            var jobs = await _jobAppService.GetAll(new GetAllJobsInput() { ProjectId = project.Id, Sorting = "Title ASC" });

            // Assert
            jobs.ShouldNotBeNull();
            jobs.Items.Count.ShouldBe(2);
            jobs.Items.First().Title.ShouldBe("A");
            jobs.Items[1].Title.ShouldBe("Z");
        }

        [Fact]
        public async Task GetAllJobs_Paging()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = await _projectAppService.Create(new CreateProjectInputDto() { Title = "test" });
            var job = await _jobAppService.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job", Description = "test job" });
            var job2 = await _jobAppService.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job2", Description = "test job2" });
            var job3 = await _jobAppService.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job3", Description = "test job3" });

            // Act
            var jobs = await _jobAppService.GetAll(new GetAllJobsInput() { ProjectId = project.Id, MaxResultCount = 1 });

            // Assert items count is 1 and total count is 2 and is sorted by creation date desc
            jobs.ShouldNotBeNull();
            jobs.Items.Count.ShouldBe(1);
            jobs.TotalCount.ShouldBe(3);
            jobs.Items[0].Title.ShouldBe("test job3");
        }

        [Fact]
        public async Task SetTitle_ReturnsJob()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = await _projectAppService.Create(new CreateProjectInputDto() { Title = "test" });
            var job = await _jobAppService.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job", Description = "test job" });

            // Act
            var job2 = await _jobAppService.SetTitle(new JobSetTitleInputDto() { Id = job.Id, Title = "test job2" });

            // Assert
            job2.ShouldNotBeNull();
            job2.Title.ShouldBe("test job2");
        }
        [Fact]
        public async Task SetDescription_ReturnsJob()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = await _projectAppService.Create(new CreateProjectInputDto() { Title = "test" });
            var job = await _jobAppService.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job", Description = "test job" });

            // Act
            var job2 = await _jobAppService.SetDescription(new JobSetDescriptionInputDto() { Id = job.Id, Description = "test job2" });

            // Assert
            job2.ShouldNotBeNull();
            job2.Description.ShouldBe("test job2");
        }
        [Fact]
        public async Task SetStatus_ReturnsJob()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = await _projectAppService.Create(new CreateProjectInputDto() { Title = "test" });
            var job = await _jobAppService.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job", Description = "test job" });

            // Act
            var job2 = await _jobAppService.SetJobStatus(new JobSetStatusInputDto() { Id = job.Id, JobStatus = Status.Done });

            // Assert
            job2.JobStatus.ShouldBe(Status.Done);
        }

        [Fact]
        public async Task SetDueDate_ReturnsJob()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = await _projectAppService.Create(new CreateProjectInputDto() { Title = "test" });
            var job = await _jobAppService.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job", Description = "test job" });
            var dueDate = DateTime.Now + TimeSpan.FromDays(1);
            // Act
            var job2 = await _jobAppService.SetDueDate(new JobSetDueDateInputDto() { Id = job.Id, DueDate = dueDate });

            // Assert
            job2.DueDate.ShouldBe(dueDate);

        }

        [Fact]
        public async Task DeleteJob_Success()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = await _projectAppService.Create(new CreateProjectInputDto() { Title = "test" });
            var job = await _jobAppService.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job", Description = "test job" });

            //act
            var response = await _jobAppService.Delete(job.Id);

            //assert
            response.ShouldBeOfType<NoContentResult>();
        }

        [Fact]
        public async Task DeleteJob_NotFound()
        {
            //act
            var response = await _jobAppService.Delete(Guid.NewGuid());

            //assert
            response.ShouldBeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task PatchOrderByDate_Success()
        {
            // Arrange
            await GetCurrentUserAsync();
            await GetCurrentTenantAsync();
            var project = await _projectAppService.Create(new CreateProjectInputDto() { Title = "test" });
            var job = await _jobAppService.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job", Description = "test job" });
            var orderByDate = DateTime.UtcNow;

            //act
            var response = await _jobAppService.PatchOrderByDate(new JobPatchOrderByDateInputDto { Id = job.Id, OrderByDate = orderByDate });

            //assert
            response.Value.OrderByDate.ShouldBe(orderByDate);

        }

        [Fact]
        public async Task PatchOrderByDate_NotFound()
        {
            //act
            var response = await _jobAppService.PatchOrderByDate(new JobPatchOrderByDateInputDto { Id = Guid.NewGuid(), OrderByDate = DateTime.UtcNow });

            //assert
            response.Result.ShouldBeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task GetJobStats_getsStatsForCurrentJobs()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = await _projectAppService.Create(new CreateProjectInputDto() { Title = "test" });
            var job = await _jobAppService.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job", Description = "test job" });
            var job2 = await _jobAppService.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job2", Description = "test job2" });
            var job3 = await _jobAppService.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job3", Description = "test job3" });

            // Act
            var jobStats = await _jobAppService.GetJobStats();

            // Assert items count is 1 and total count is 2 and is sorted by creation date desc
            jobStats.ShouldNotBeNull();
            jobStats.TotalJobs.ShouldBe(3);
            jobStats.TotalCompletedJobs.ShouldBe(0);
            jobStats.TotalInProgressJobs.ShouldBe(0);
            jobStats.TotalOpenJobs.ShouldBe(3);
            jobStats.TotalCompletedJobsPerMonth.Count.ShouldBe(0);
        }

        
    }
}