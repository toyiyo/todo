using System.Threading.Tasks;
using toyiyo.todo.Models.TokenAuth;
using toyiyo.todo.Web.Controllers;
using Shouldly;
using Xunit;
using System;

namespace toyiyo.todo.Web.Tests.Controllers
{
    public class HomeController_Tests: todoWebTestBase
    {
        [Fact]
        public async Task Index_Test()
        {
            await AuthenticateAsync(null, new AuthenticateModel
            {
                UserNameOrEmailAddress = "admin",
                Password = Environment.GetEnvironmentVariable("DefaultPassword")
            });

            //Act
            var response = await GetResponseAsStringAsync(
                GetUrl<HomeController>(nameof(HomeController.IndexAsync))
            );

            //Assert
            response.ShouldNotBeNullOrEmpty();
        }
    }
}