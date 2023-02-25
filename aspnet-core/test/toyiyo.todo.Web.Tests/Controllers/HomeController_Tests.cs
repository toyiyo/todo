using System.Threading.Tasks;
using toyiyo.todo.Models.TokenAuth;
using toyiyo.todo.Web.Controllers;
using Shouldly;
using Xunit;
using System;
using Abp.MultiTenancy;

namespace toyiyo.todo.Web.Tests.Controllers
{
    public class HomeController_Tests : todoWebTestBase
    {
        [Fact]
        public async Task Index_NoTenant_ThrowsUnauthorized()
        {
            await AuthenticateAsync(null, new AuthenticateModel
            {
                UserNameOrEmailAddress = "admin",
                Password = Environment.GetEnvironmentVariable("DefaultPassword")
            });

            await Assert.ThrowsAnyAsync<Exception>(async () => await GetResponseAsStringAsync(
                GetUrl<HomeController>("Index")));
        }

        [Fact]
        public async Task Index_TenantSet_ReturnsOk()
        {
            SetDefaultTenant();
            await AuthenticateAsync(AbpTenantBase.DefaultTenantName, new AuthenticateModel
            {
                UserNameOrEmailAddress = "admin",
                Password = Environment.GetEnvironmentVariable("DefaultPassword")
            });

           await GetResponseAsync(GetUrl<HomeController>("Index"));
        }
    }
}