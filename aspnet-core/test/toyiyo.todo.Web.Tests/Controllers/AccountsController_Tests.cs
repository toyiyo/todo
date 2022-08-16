using System.Threading.Tasks;
using toyiyo.todo.Models.TokenAuth;
using toyiyo.todo.Web.Controllers;
using Shouldly;
using Xunit;

namespace toyiyo.todo.Web.Tests.Controllers
{
    public class Controller_Tests: todoWebTestBase
    {
        [Fact]
        public async Task AccessRegisterCompanyPage_Test()
        {

            //Act
            var response = await GetResponseAsStringAsync(
                GetUrl<AccountController>(nameof(AccountController.RegisterCompanyAdmin))
            );

            //Assert
            response.ShouldNotBeNullOrEmpty();
        }
    }
}