using System.Threading.Tasks;
using toyiyo.todo.Models.TokenAuth;
using toyiyo.todo.Web.Controllers;
using Shouldly;
using Xunit;
using System;
using toyiyo.todo.Jobs;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http;
using toyiyo.todo.Web.Models.Jobs;
using System.Collections.Generic;
using toyiyo.todo.Projects;
using toyiyo.todo.Projects.Dto;
using Abp.Application.Services.Dto;
using toyiyo.todo.Jobs.Dto;
using Abp.MultiTenancy;
using Abp.Domain.Repositories;

namespace toyiyo.todo.Web.Tests.Controllers
{
    public class JobsController_Tests : todoWebTestBase
    {
        private readonly IJobAppService _jobAppService;
        private readonly IProjectAppService _projectAppService;
        private readonly IProjectManager _projectManager;
        private readonly IJobManager _jobManager;
        private readonly IRepository<Project, Guid> _projectRepository;
        private readonly IRepository<Job, Guid> _jobRepository;

        public JobsController_Tests() { 
            _jobAppService = Resolve<IJobAppService>(); 
            _projectAppService = Resolve<IProjectAppService>(); 
            _projectManager = Resolve<IProjectManager>();
            _jobManager = Resolve<IJobManager>();
            _projectRepository = Resolve<IRepository<Project, Guid>>();
            _jobRepository = Resolve<IRepository<Job, Guid>>();
            }

        [Fact]
        public async Task JobIndex_InvalidProject_ReturnsNotFound()
        {
            //arrange
            await AuthenticateAsync(null, new AuthenticateModel
            {
                UserNameOrEmailAddress = "admin",
                Password = Environment.GetEnvironmentVariable("DefaultPassword")
            });

            //act - load the page
            var response = await GetResponseAsStringAsync(
                GetUrl<JobsController>(nameof(JobsController.Index), new { id = Guid.NewGuid() }), System.Net.HttpStatusCode.NotFound
            );

            //Assert
            response.ShouldBeNullOrEmpty();
        }
        // Assuming you have references to the required testing frameworks and namespaces

        [Fact]
        public async Task Index_ReturnsView()
        {
            // Arrange
            var projectId = Guid.NewGuid();
            var jobId = Guid.NewGuid();

            // Create an instance of the controller
            var controller = new JobsController(_jobAppService);

            // Act
            // Invoke the action method with the required parameters
            var result = await controller.Index(projectId, jobId);

            // Assert
            // Check the returned result and its properties
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);  // Check if view name is null or empty

            Assert.True(viewResult.ViewData.ContainsKey("ProjectId"));
            Assert.True(viewResult.ViewData.ContainsKey("JobId"));
            Assert.Equal(projectId, viewResult.ViewData["ProjectId"]);
            Assert.Equal(jobId, viewResult.ViewData["JobId"]);
        }

        //todo - follow testing guidelines here https://aspnetboilerplate.com/Pages/Documents/Articles/Unit-Testing-with-Entity-Framework,-xUnit-Effort/index.html?searchKey=mock
        
        // [Fact]
        // public async Task EditModal_ReturnsPartialViewWithModel()
        // {
        //     // Arrange
        //     SetDefaultTenant();
        //     await AuthenticateAsync(AbpTenantBase.DefaultTenantName, new AuthenticateModel
        //     {
        //         UserNameOrEmailAddress = "admin",
        //         Password = Environment.GetEnvironmentVariable("DefaultPassword")
        //     });
        //     //we don't mock the app service, we want to test the actual services so we use them to create the test data.  This goes into a temp database - https://aspnetboilerplate.com/Pages/Documents/Articles/Unit-Testing-with-Entity-Framework,-xUnit-Effort/index.html?searchKey=mock
        //     var project = await _projectAppService.Create(new CreateProjectInputDto() { Title = "project.Title", Description = "project.Description"});
        //     var parentJob = await _jobAppService.Create(new JobCreateInputDto(){ Title = "parent job", Description = "parent job description", ProjectId = project.Id});
        //     var childJob1 = await _jobAppService.Create(new JobCreateInputDto(){ Title = "child job 1", Description = "child job 1 description", ProjectId = project.Id, ParentId = parentJob.Id});
        //     var childJob2 = await _jobAppService.Create(new JobCreateInputDto(){ Title = "child job 2", Description = "child job 2 description", ProjectId = project.Id, ParentId = parentJob.Id});
        //     var subTasks = new List<JobDto>() {childJob1, childJob2};
        //     var controller = new JobsController(_jobAppService);

        //     // Act
        //     var result = await controller.EditModal(parentJob.Id);

        //     // Assert
        //     var partialViewResult = Assert.IsType<PartialViewResult>(result);
        //     Assert.Equal("_EditModal", partialViewResult.ViewName);

        //     var model = Assert.IsType<EditJobModalViewModel>(partialViewResult.Model);
        //     Assert.Equal(parentJob.Id, model.Id);
        //     Assert.Equal(parentJob.Title, model.Title);

        //     var subTaskModels = Assert.IsType<List<EditJobSubTaskModalViewModel>>(controller.ViewBag.SubTasks);
        //     Assert.Equal(subTasks.Count, subTaskModels.Count);
        //     Assert.Equal(subTasks[0].Title, subTaskModels[0].Title);
        //     Assert.Equal(subTasks[1].Title, subTaskModels[1].Title);
        // }
    }
}