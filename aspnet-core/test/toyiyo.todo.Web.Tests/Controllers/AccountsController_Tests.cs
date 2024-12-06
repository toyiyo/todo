using System.Threading.Tasks;
using toyiyo.todo.Web.Controllers;
using Shouldly;
using Xunit;
using System.Net.Http;
using System.Net;
using System.Collections.Generic;
using toyiyo.todo.Invitations;
using toyiyo.todo.Invitations.Dto;
using Abp.ObjectMapping;
using toyiyo.todo.Authorization.Users;
using Abp.Authorization.Users;
using toyiyo.todo.MultiTenancy;
using System.Linq;
using System;


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
            result.Content.ReadAsStringAsync().Result.ShouldContain("Invalid or expired invitation token.");
        }

        [Fact]
        public async Task Register_WithValidInvitation_ShouldSucceed()
        {
            // Arrange
            SetDefaultTenant();

            // Create a valid invitation with a token and email
            var invitation = await CreateValidInvitation(10, "test@example.com");

            // Force subscription limit to be reached
            await SetTenantSubscriptionSeats(10, GetTenant());

            // Construct the URL with the token and tenant in the query string
            var url = GetUrl<AccountController>(nameof(AccountController.Register)) + $"?token={WebUtility.UrlEncode(invitation.Token)}&tenant={WebUtility.UrlEncode(GetTenant().TenancyName)}";


            var content = new Dictionary<string, string>
            {
                {"Name", "Test"},
                {"Surname", "User"},
                {"UserName", invitation.Email},
                {"EmailAddress", invitation.Email},
                {"Password", "testPassword123"},
                {"Token", invitation.Token}
            };

            var data = new FormUrlEncodedContent(content);

            // Act
            var result = await Client.PostAsync(url, data);

            // Assert
            var response = await result.Content.ReadAsStringAsync();
            result.StatusCode.ShouldBe(HttpStatusCode.Redirect);
            Console.WriteLine("location header is {0}", result.Headers.ToString());
            Console.WriteLine("page content is {0}", response);
            result.Headers.Location.ToString().ShouldContain("/");
        }

        [Fact]
        public async Task Register_WithInvalidInvitation_ShouldFail()
        {
            // Arrange
            SetDefaultTenant();
            await SetTenantSubscriptionSeats(10, GetTenant()); // Set reasonable limit
            var url = GetUrl<AccountController>(nameof(AccountController.Register));

            var content = new Dictionary<string, string>
            {
                {"Name", "Test"},
                {"Surname", "User"},
                {"UserName", "test@example.com"},
                {"EmailAddress", "test@example.com"},
                {"Password", "testPassword123"},
                {"Token", "invalid-token"}
            };

            var data = new FormUrlEncodedContent(content);

            // Act
            var result = await Client.PostAsync(url, data);

            // Assert
            result.StatusCode.ShouldBe(HttpStatusCode.OK);
            var responseContent = await result.Content.ReadAsStringAsync();
            responseContent.ShouldContain("Invalid or expired invitation token");
        }

        [Fact]
        public async Task Register_WithMismatchedEmail_ShouldFail()
        {
            // Arrange
            SetDefaultTenant();
            await SetTenantSubscriptionSeats(10, GetTenant()); // Set reasonable limit
            var url = GetUrl<AccountController>(nameof(AccountController.Register));
            var invitation = await CreateValidInvitation(10, "test@example.com");

            var content = new Dictionary<string, string>
            {
                {"Name", "Test"},
                {"Surname", "User"},
                {"UserName", "different@example.com"},
                {"EmailAddress", "different@example.com"},
                {"Password", "testPassword123"},
                {"Token", invitation.Token}
            };

            var data = new FormUrlEncodedContent(content);

            // Act
            var result = await Client.PostAsync(url, data);

            // Assert
            result.StatusCode.ShouldBe(HttpStatusCode.OK);
            var responseContent = await result.Content.ReadAsStringAsync();
            responseContent.ShouldContain("Invalid invitation token for this email");
        }

        [Fact]
        public async Task Register_WithExceededSubscriptionLimit_ShouldFail()
        {
            // Arrange
            SetDefaultTenant();

            // Create a valid invitation with a token and email
            var invitation = await CreateValidInvitation(10, "test@example.com");

            // Force subscription limit to be reached
            await SetTenantSubscriptionSeats(1, GetTenant());

            // Construct the URL with the token and tenant in the query string
            var url = GetUrl<AccountController>(nameof(AccountController.Register)) + $"?token={WebUtility.UrlEncode(invitation.Token)}&tenant={WebUtility.UrlEncode(GetTenant().TenancyName)}";


            var content = new Dictionary<string, string>
            {
                {"Name", "Test"},
                {"Surname", "User"},
                {"UserName", invitation.Email},
                {"EmailAddress", invitation.Email},
                {"Password", "testPassword123"}
            };

            var data = new FormUrlEncodedContent(content);

            // Act
            var result = await Client.PostAsync(url, data);

            // Assert
            result.StatusCode.ShouldBe(HttpStatusCode.OK);
            var responseContent = await result.Content.ReadAsStringAsync();
            responseContent.ShouldContain("Subscription limit reached", Case.Insensitive);
        }

        private Tenant GetTenant()
        {
            return GetDefaultTenant();
        }

        private async Task SetTenantSubscriptionSeats(int seats, Tenant tenant)
        {
            var tenantManager = Resolve<TenantManager>();
            await tenantManager.SetSubscriptionSeats(tenant, seats);
        }

        private async Task<UserInvitation> CreateValidInvitation(int seats, string email)
        {
            var tenant = GetTenant();
            await SetTenantSubscriptionSeats(seats, tenant); // Ensure invitation can be created
            var userInvitationManager = Resolve<IUserInvitationManager>();
            var adminUser = await GetDefaultTenantAdmin();

            return await userInvitationManager.CreateInvitationAsync(
                tenant,
                email,
                adminUser
            );
        }

        private async Task<User> GetDefaultTenantAdmin()
        {
            var userManager = Resolve<UserManager>();
            return await userManager.FindByNameAsync(AbpUserBase.AdminUserName);
        }

        private Tenant GetDefaultTenant()
        {
            var tenantManager = Resolve<TenantManager>();
            return UsingDbContext(context => context.Tenants.First());
        }
    }
}