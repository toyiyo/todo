using System.Threading.Tasks;
using toyiyo.todo.Web.Controllers;
using Shouldly;
using Xunit;
using System.Net.Http;
using System.Text;
using System.Net;
using Abp.Json;
using System.Collections.Generic;
using toyiyo.todo.Models.TokenAuth;
using System;
using System.Linq;

namespace toyiyo.todo.Web.Tests.Controllers
{
    public class Controller_Tests : todoWebTestBase
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

        [Fact]
        public async Task RegisterTenantCompany()
        {
            var url = GetUrl<AccountController>(nameof(AccountController.RegisterCompanyAdmin));

            var content = new Dictionary<string, string>();
            content.Add("EmailAddress", "joe@google.com");
            content.Add("Password", "fakepassword");
            content.Add("TenancyName", "google");
            content.Add("Name", "Alphabet Inc");

            var data = new FormUrlEncodedContent(content);

            var result = await Client.PostAsync(url, data);

            //when we create a tenant with admin accounts, we get redirected
            result.StatusCode.ShouldBe(HttpStatusCode.Redirect);

        }

        [Fact]
        public async Task RegisterAsUserOnExistingTenant_TenantSelfRegistrationIsDisabled()
        {
            SetDefaultTenant();

            var url = GetUrl<AccountController>(nameof(AccountController.Register));

            var content = new Dictionary<string, string>();
            content.Add("Name", "joe");
            content.Add("Surname", "bloggs");
            content.Add("UserName", "joe@google.com");
            content.Add("EmailAddress", "joe@google.com");
            content.Add("Password", "fakepassword");

            var data = new FormUrlEncodedContent(content);

            var result = await Client.PostAsync(url, data);

            result.StatusCode.ShouldBe(HttpStatusCode.OK);
            result.Content.ReadAsStringAsync().Result.ShouldContain("Self registration is disabled");
        }
}