using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using toyiyo.todo.Projects;
using toyiyo.todo.Sessions;
using toyiyo.todo.Jobs;
using Xunit;
using static toyiyo.todo.Jobs.Job;
using System;

namespace toyiyo.todo.Tests.Jobs
{
    public class JobAppServiceTests : todoTestBase
    {
        private readonly IProjectAppService _projectAppService;
        private readonly ISessionAppService _sessionAppService;
        private readonly IJobAppService _jobAppServices;
        public JobAppServiceTests()
        {
            _projectAppService = Resolve<IProjectAppService>();
            _sessionAppService = Resolve<ISessionAppService>();
            _jobAppServices = Resolve<IJobAppService>();
        }
        [Fact]
        public async Task CreateJob_ReturnsNewJob()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = await _projectAppService.Create(new CreateProjectInputDto() { Title = "test" });

            // Act
            var job = await _jobAppServices.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job", Description = "test job" });

            // Assert
            job.ShouldNotBeNull();
            job.Title.ShouldBe("test job");
            job.Description.ShouldBe("test job");
            job.JobStatus.ShouldBe(Status.Open);
            job.Assignee.Id.ShouldBe(currentUser.Id);
            job.Owner.Id.ShouldBe(currentUser.Id);
        }

        [Fact]
        public async Task GetJob_ReturnsJob()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = await _projectAppService.Create(new CreateProjectInputDto() { Title = "test" });
            var job = await _jobAppServices.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job", Description = "test job" });

            // Act
            var job2 = await _jobAppServices.Get(job.Id);

            // Assert
            job2.ShouldNotBeNull();
            job2.Title.ShouldBe("test job");
        }

        [Fact]
        public async Task GetAllJobs_FilterByJobStatus()
        {
            // Arrange
            var currentUser = await GetCurrentUserAsync();
            var currentTenant = await GetCurrentTenantAsync();
            var project = await _projectAppService.Create(new CreateProjectInputDto() { Title = "test" });
            var job = await _jobAppServices.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job", Description = "test job" });
            await _jobAppServices.SetJobStatus(new JobSetStatusInputDto() { Id = job.Id, JobStatus = Status.Done });

            // Act
            var openJobs = await _jobAppServices.GetAll(new GetAllJobsInput() { JobStatus = Status.Open });
            var doneJobs = await _jobAppServices.GetAll(new GetAllJobsInput() { JobStatus = Status.Done });

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
            var job = await _jobAppServices.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job", Description = "test job" });
            var job2 = await _jobAppServices.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job2", Description = "test job2" });

            // Act
            var jobs = await _jobAppServices.GetAll(new GetAllJobsInput() { ProjectId = project.Id });

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
            var job = await _jobAppServices.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job", Description = "test job" });
            var job2 = await _jobAppServices.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job2", Description = "test job2" });

            // Act
            var jobs = await _jobAppServices.GetAll(new GetAllJobsInput() { ProjectId = project.Id, Sorting = "CreationTime ASC" });

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
            var job = await _jobAppServices.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "A", Description = "test job" });
            var job2 = await _jobAppServices.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "Z", Description = "test job2" });

            // Act
            var jobs = await _jobAppServices.GetAll(new GetAllJobsInput() { ProjectId = project.Id, Sorting = "Title ASC" });

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
            var job = await _jobAppServices.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job", Description = "test job" });
            var job2 = await _jobAppServices.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job2", Description = "test job2" });
            var job3 = await _jobAppServices.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job3", Description = "test job3" });

            // Act
            var jobs = await _jobAppServices.GetAll(new GetAllJobsInput() { ProjectId = project.Id, MaxResultCount = 1 });

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
            var job = await _jobAppServices.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job", Description = "test job" });

            // Act
            var job2 = await _jobAppServices.SetTitle(new JobSetTitleInputDto() { Id = job.Id, Title = "test job2" });

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
            var job = await _jobAppServices.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job", Description = "test job" });

            // Act
            var job2 = await _jobAppServices.SetDescription(new JobSetDescriptionInputDto() { Id = job.Id, Description = "test job2" });

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
            var job = await _jobAppServices.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job", Description = "test job" });

            // Act
            var job2 = await _jobAppServices.SetJobStatus(new JobSetStatusInputDto() { Id = job.Id, JobStatus = Status.Done });

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
            var job = await _jobAppServices.Create(new JobCreateInputDto() { ProjectId = project.Id, Title = "test job", Description = "test job" });
            var dueDate = DateTime.Now + TimeSpan.FromDays(1);
            // Act
            var job2 = await _jobAppServices.SetDueDate(new JobSetDueDateInputDto() { Id = job.Id, DueDate = dueDate });

            // Assert
            job2.DueDate.ShouldBe(dueDate);
           
        }
    }
}