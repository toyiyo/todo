using System.Threading.Tasks;
using toyiyo.todo.Models.TokenAuth;
using toyiyo.todo.Web.Controllers;
using Shouldly;
using Xunit;
using System;

namespace toyiyo.todo.Web.Tests.Controllers
{
    public class UsersController_Tests: todoWebTestBase
    {
        [Fact]
        public async Task AccessPasswordPage_AsAdmin_Test()
        {
            await AuthenticateAsync(null, new AuthenticateModel
            {
                UserNameOrEmailAddress = "admin",
                Password = Environment.GetEnvironmentVariable("DefaultPassword")
            });

            //Act
            var response = await GetResponseAsStringAsync(
                GetUrl<UsersController>(nameof(UsersController.ChangePassword))
            );

            //Assert
            response.ShouldNotBeNullOrEmpty();
        }
    }
}