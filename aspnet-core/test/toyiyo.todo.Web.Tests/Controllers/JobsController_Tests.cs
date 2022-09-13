using System.Threading.Tasks;
using toyiyo.todo.Models.TokenAuth;
using toyiyo.todo.Web.Controllers;
using Shouldly;
using Xunit;
using System;
using System.Net.Http;
using System.Collections.Generic;
using toyiyo.todo.Projects;
using toyiyo.todo.Sessions;
using Newtonsoft.Json;
using toyiyo.todo.Projects.Dto;
using Newtonsoft.Json.Serialization;

namespace toyiyo.todo.Web.Tests.Controllers
{
    public class JobsController_Tests : todoWebTestBase
    {
        private readonly IProjectAppService _projectAppService;

        public JobsController_Tests() { _projectAppService = Resolve<IProjectAppService>(); }

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

        // [Fact]
        // public async Task JobIndex_ValidProject_ReturnsOk()
        // {
        //     //arrange
        //     LoginAsDefaultTenantAdmin();
        //     Console.WriteLine("user id {0}", AbpSession.UserId );
        //     Console.WriteLine("tenant id {0}", AbpSession.TenantId );
        //     //create a project via the API so we can use it in the controller
        //     var createProjectUrl = GetUrl<ProjectAppService>(nameof(ProjectAppService.Create));

        //     var content = new Dictionary<string, string>();
        //     content.Add("title", "test description");

        //     var data = new FormUrlEncodedContent(content);
        //     var projectResponse = await Client.PostAsync(createProjectUrl, data);

        //     Console.WriteLine("response {0}", await projectResponse.Content.ReadAsStringAsync());
        //     //todo: figure out why we get an empty response here.  If this fails, go back to attempting to create a project via the API instead.
        //     var projectDtoResponse = JsonConvert.DeserializeObject<ProjectDto>(await projectResponse.Content.ReadAsStringAsync(), new JsonSerializerSettings
        //     {
        //         ContractResolver = new CamelCasePropertyNamesContractResolver()
        //     }); 

        //     //Console.WriteLine("Project DTO id = {0}", projectDtoResponse.Id);

        //     //act - load the page
        //     var response = await GetResponseAsStringAsync(
        //         GetUrl<JobsController>(nameof(JobsController.Index), new { id = projectDtoResponse.Id }), System.Net.HttpStatusCode.OK
        //     );

        //     //Assert
        //     response.ShouldNotBeNullOrEmpty();
        // }
    }
}