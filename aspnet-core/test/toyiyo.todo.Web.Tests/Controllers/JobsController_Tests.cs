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
using toyiyo.todo.Authorization.Roles;
using Abp.Domain.Uow;

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
        private readonly RoleManager _roleManager;

        public JobsController_Tests()
        {
            _jobAppService = Resolve<IJobAppService>();
            _projectAppService = Resolve<IProjectAppService>();
            _projectManager = Resolve<IProjectManager>();
            _jobManager = Resolve<IJobManager>();
            _projectRepository = Resolve<IRepository<Project, Guid>>();
            _jobRepository = Resolve<IRepository<Job, Guid>>();
            _roleManager = Resolve<RoleManager>();
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
            //Arrange
            var unitOfWorkManager = Resolve<IUnitOfWorkManager>();

            SeedDb();

            var projectId = this.Project.Id;
            var jobId = Guid.NewGuid();

            // Create an instance of the controller
            var controller = new JobsController(_jobAppService, _projectAppService);

            // Act
            using var unitOfWork = unitOfWorkManager.Begin();
            // Invoke the action method with the required parameters
            var result = await controller.Index(projectId, jobId);

            // Complete the unit of work
            unitOfWork.Complete();

            // Assert
            // Check the returned result and its properties
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);  // Check if view name is null or empty

            Assert.True(viewResult.ViewData.ContainsKey("ProjectId"));
            Assert.True(viewResult.ViewData.ContainsKey("JobId"));
            Assert.True(viewResult.ViewData.ContainsKey("ProjectTitle"));
            Assert.Equal("test project", viewResult.ViewData["ProjectTitle"]);
            Assert.Equal(projectId, viewResult.ViewData["ProjectId"]);
            Assert.Equal(jobId, viewResult.ViewData["JobId"]);

        }

        //todo - follow testing guidelines here https://aspnetboilerplate.com/Pages/Documents/Articles/Unit-Testing-with-Entity-Framework,-xUnit-Effort/index.html?searchKey=mock

    }
}