using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Uow;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using toyiyo.todo.Authorization.Users;
using toyiyo.todo.Jobs;
using toyiyo.todo.Jobs.Dto;
using toyiyo.todo.Projects;
using toyiyo.todo.Projects.Dto;
using toyiyo.todo.Sessions;
using Xunit;
using static toyiyo.todo.Jobs.Job;

namespace toyiyo.todo.Tests.Projects
{
    public class ProjectProgressTests : todoTestBase
    {
        private readonly IProjectAppService _projectAppService;
        private readonly IJobAppService _jobAppService;

        public ProjectProgressTests()
        {
            _projectAppService = Resolve<IProjectAppService>();
            _jobAppService = Resolve<IJobAppService>();
            LoginAsDefaultTenantAdmin();
        }

        private async Task<Project> GetProjectEntityById(Guid id)
        {
            return await UsingDbContext(async context => 
                await context.Projects
                    .Include(p => p.Jobs)
                    .FirstOrDefaultAsync(p => p.Id == id)
            );
        }

        private async Task<Project> CreateProjectWithMixedJobs()
        {
            var project = await _projectAppService.Create(new CreateProjectInputDto { Title = "Test Project" });

            // Create Epics
            var epic1 = await _jobAppService.Create(new JobCreateInputDto 
            { 
                ProjectId = project.Id, 
                Title = "Epic 1",
                Level = JobLevel.Epic
            });
            await _jobAppService.SetJobStatus(new JobSetStatusInputDto { Id = epic1.Id, JobStatus = Status.Done });

            var epic2 = await _jobAppService.Create(new JobCreateInputDto 
            { 
                ProjectId = project.Id, 
                Title = "Epic 2",
                Level = JobLevel.Epic
            });
            await _jobAppService.SetJobStatus(new JobSetStatusInputDto { Id = epic2.Id, JobStatus = Status.InProgress });

            // Create Tasks with different statuses
            for (int i = 0; i < 3; i++)
            {
                var task = await _jobAppService.Create(new JobCreateInputDto 
                { 
                    ProjectId = project.Id, 
                    Title = $"Completed Task {i}",
                    Level = JobLevel.Task
                });
                await _jobAppService.SetJobStatus(new JobSetStatusInputDto { Id = task.Id, JobStatus = Status.Done });
            }

            for (int i = 0; i < 2; i++)
            {
                var task = await _jobAppService.Create(new JobCreateInputDto 
                { 
                    ProjectId = project.Id, 
                    Title = $"In Progress Task {i}",
                    Level = JobLevel.Task
                });
                await _jobAppService.SetJobStatus(new JobSetStatusInputDto { Id = task.Id, JobStatus = Status.InProgress });
            }

            // Create Bugs
            for (int i = 0; i < 3; i++)
            {
                await _jobAppService.Create(new JobCreateInputDto 
                { 
                    ProjectId = project.Id, 
                    Title = $"Bug {i}",
                    Level = JobLevel.Bug
                });
            }

            return await GetProjectEntityById(project.Id);
        }

        private async Task<Project> CreateProjectWithDeletedJobs()
        {
            var project = await _projectAppService.Create(new CreateProjectInputDto { Title = "Test Project" });

            // Create jobs with different statuses
            var job1 = await _jobAppService.Create(new JobCreateInputDto 
            { 
                ProjectId = project.Id, 
                Title = "Task 1"
            });
            await _jobAppService.SetJobStatus(new JobSetStatusInputDto { Id = job1.Id, JobStatus = Status.Done });

            var job2 = await _jobAppService.Create(new JobCreateInputDto 
            { 
                ProjectId = project.Id, 
                Title = "Task 2"
            });
            await _jobAppService.SetJobStatus(new JobSetStatusInputDto { Id = job2.Id, JobStatus = Status.InProgress });

            var job3 = await _jobAppService.Create(new JobCreateInputDto 
            { 
                ProjectId = project.Id, 
                Title = "Task 3"
            });

            var jobToDelete = await _jobAppService.Create(new JobCreateInputDto 
            { 
                ProjectId = project.Id, 
                Title = "To Delete"
            });

            await _jobAppService.Delete(jobToDelete.Id);
            return await GetProjectEntityById(project.Id);
        }

        private async Task<Project> CreateProjectWithDueDates()
        {
            var project = await _projectAppService.Create(new CreateProjectInputDto { Title = "Test Project" });

            // Create jobs with different due dates
            await _jobAppService.Create(new JobCreateInputDto 
            { 
                ProjectId = project.Id, 
                Title = "Task 1",
                DueDate = DateTime.UtcNow.AddDays(10)
            });

            await _jobAppService.Create(new JobCreateInputDto 
            { 
                ProjectId = project.Id, 
                Title = "Task 2",
                DueDate = DateTime.UtcNow.AddDays(20)
            });

            await _jobAppService.Create(new JobCreateInputDto 
            { 
                ProjectId = project.Id, 
                Title = "Task 3",
                DueDate = DateTime.UtcNow.AddDays(30)
            });

            return await GetProjectEntityById(project.Id);
        }

        private async Task<Project> CreateProjectWithAllTasksCompleted()
        {
            var project = await _projectAppService.Create(new CreateProjectInputDto { Title = "Test Project" });

            // Create multiple tasks and mark them as completed
            for (int i = 0; i < 5; i++)
            {
                var task = await _jobAppService.Create(new JobCreateInputDto 
                { 
                    ProjectId = project.Id, 
                    Title = $"Completed Task {i}"
                });
                await _jobAppService.SetJobStatus(new JobSetStatusInputDto { Id = task.Id, JobStatus = Job.Status.Done });
            }

            return await GetProjectEntityById(project.Id);
        }

        [Fact]
        public async Task Should_Calculate_Project_Progress_With_Mixed_Jobs()
        {
            var project = await CreateProjectWithMixedJobs();
            var progress = ProjectProgress.Calculate(project);

            progress.TotalJobCount.ShouldBe(8); // 5 tasks + 3 bugs
            progress.CompletedTasks.ShouldBe(3);
            progress.InProgressTasks.ShouldBe(2);
            progress.BacklogTasks.ShouldBe(3);
            progress.EpicCount.ShouldBe(2);
            progress.CompletedEpics.ShouldBe(1);
            progress.TaskCount.ShouldBe(5);
            progress.BugCount.ShouldBe(3);
            progress.TotalTasksPercentage.ShouldBe(37.50m); // 3 completed tasks out of 8 total jobs
            progress.InProgressPercentage.ShouldBe(25.00m); // 2 in-progress tasks out of 8 total jobs
            progress.DueDate.ShouldNotBeNull();
            progress.HealthStatus.ShouldNotBeNull();
        }

        [Fact]
        public async Task Should_Calculate_Project_Progress_With_Deleted_Jobs()
        {
            var project = await CreateProjectWithDeletedJobs();
            var progress = ProjectProgress.Calculate(project);

            progress.TotalJobCount.ShouldBe(3);
            progress.CompletedTasks.ShouldBe(1);
            progress.InProgressTasks.ShouldBe(1);
            progress.BacklogTasks.ShouldBe(1);
            progress.EpicCount.ShouldBe(0);
            progress.CompletedEpics.ShouldBe(0);
            progress.TaskCount.ShouldBe(3);
            progress.BugCount.ShouldBe(0);
            progress.TotalTasksPercentage.ShouldBe(33.33m);
            progress.InProgressPercentage.ShouldBe(33.33m);
            progress.DueDate.ShouldNotBeNull();
            progress.HealthStatus.ShouldNotBeNull();
        }

        [Fact]
        public async Task Should_Calculate_Project_Progress_With_Due_Dates()
        {
            var project = await CreateProjectWithDueDates();
            var progress = ProjectProgress.Calculate(project);

            progress.TotalJobCount.ShouldBe(3);
            progress.CompletedTasks.ShouldBe(0);
            progress.InProgressTasks.ShouldBe(0);
            progress.BacklogTasks.ShouldBe(3);
            progress.EpicCount.ShouldBe(0);
            progress.CompletedEpics.ShouldBe(0);
            progress.TaskCount.ShouldBe(3);
            progress.BugCount.ShouldBe(0);
            progress.TotalTasksPercentage.ShouldBe(0);
            progress.InProgressPercentage.ShouldBe(0);
            progress.DueDate.ShouldNotBeNull();
            progress.HealthStatus.ShouldNotBeNull();
        }

        [Fact]
        public async Task Should_Calculate_Project_Progress_With_All_Tasks_Completed()
        {
            var project = await CreateProjectWithAllTasksCompleted();
            var progress = ProjectProgress.Calculate(project);

            progress.TotalJobCount.ShouldBe(5);
            progress.CompletedTasks.ShouldBe(5);
            progress.InProgressTasks.ShouldBe(0);
            progress.BacklogTasks.ShouldBe(0);
            progress.EpicCount.ShouldBe(0);
            progress.CompletedEpics.ShouldBe(0);
            progress.TaskCount.ShouldBe(5);
            progress.BugCount.ShouldBe(0);
            progress.TotalTasksPercentage.ShouldBe(100);
            progress.InProgressPercentage.ShouldBe(0);
            progress.DueDate.ShouldNotBeNull();
            progress.HealthStatus.ShouldNotBeNull();
        }
    }
}
