using System.Threading.Tasks;
using Shouldly;
using Xunit;
using toyiyo.todo.Users;
using Abp.UI;
using toyiyo.todo.MultiTenancy;
using toyiyo.todo.Invitations;
using toyiyo.todo.Invitations.Dto;
using System;
using System.Collections.Generic;
using Abp.Net.Mail;

namespace toyiyo.todo.Tests.Invitations
{
    public class UserInvitationServiceTests : todoTestBase
    {
        private readonly IUserInvitationAppService _userInvitationService;
        private readonly TenantManager _tenantManager;
        private readonly IEmailSender _emailSender;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

        public UserInvitationServiceTests()
        {
            // Register IUserInvitationService
            _userInvitationService = Resolve<IUserInvitationAppService>();
            _tenantManager = Resolve<TenantManager>();
            _emailSender = Resolve<IEmailSender>();
            _configuration = Resolve<Microsoft.Extensions.Configuration.IConfiguration>();
            // Call LoginAsDefaultTenantAdmin before each test
            LoginAsDefaultTenantAdmin();
        }

        [Fact]
        public async Task Should_Create_Invitation_Successfully()
        {
            //set tenant's subscription seats
            LoginAsDefaultTenantAdmin();
            var currentTenant = await GetCurrentTenantAsync();
            await _tenantManager.SetSubscriptionSeats(currentTenant, 1000);
            var input = new CreateUserInvitationDto
            {
                Email = "test@example.com"
            };

            // Act
            var result = await _userInvitationService.CreateInvitationAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.Email.ShouldBe(input.Email);
            result.Status.ShouldBe(InvitationStatus.Pending);
        }

        [Fact]
        public async Task Should_Not_Allow_Duplicate_Invitations()
        {
            // Arrange
            LoginAsDefaultTenantAdmin();
            var currentTenant = await GetCurrentTenantAsync();
            await _tenantManager.SetSubscriptionSeats(currentTenant, 1000);
            var input = new CreateUserInvitationDto
            {
                Email = "duplicate@example.com"
            };

            // Act & Assert
            await _userInvitationService.CreateInvitationAsync(input);

            await Should.ThrowAsync<InvalidOperationException>(async () =>
            {
                await _userInvitationService.CreateInvitationAsync(input);
            });
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("invalid-email")]
        public async Task Should_Validate_Email_Format(string invalidEmail)
        {
            // Arrange
            var input = new CreateUserInvitationDto
            {
                Email = invalidEmail
            };

            // Act & Assert
            await Should.ThrowAsync<Abp.Runtime.Validation.AbpValidationException>(async () =>
            {
                await _userInvitationService.CreateInvitationAsync(input);
            });
        }

        [Fact]
        public async Task Should_Validate_Subscription_Limit()
        {
            // Arrange
            LoginAsDefaultTenantAdmin();
            var currentTenant = await GetCurrentTenantAsync();
            await _tenantManager.SetSubscriptionSeats(currentTenant, 1);
            var input = new CreateUserInvitationDto
            {
                Email = "test@example.com"
            };

            await Should.ThrowAsync<InvalidOperationException>(async () =>
            {
                await _userInvitationService.CreateInvitationAsync(input);
            });
        }
        [Fact]
        public async Task Should_Get_All_Invitations()
        {
            // Arrange
            LoginAsDefaultTenantAdmin();
            var currentTenant = await GetCurrentTenantAsync();
            await _tenantManager.SetSubscriptionSeats(currentTenant, 1000);
            var input = new CreateUserInvitationDto
            {
                Email = "test@example.com"
            };
            var invitation = await _userInvitationService.CreateInvitationAsync(input);

            // Act
            var getAllInput = new GetAllUserInvitationsInput();
            var result = await _userInvitationService.GetAll(getAllInput);

            // Assert
            result.ShouldNotBeNull();
            result.TotalCount.ShouldBeGreaterThan(0);
            result.Items.ShouldContain(i => i.Email == invitation.Email);
        }
        [Fact]
        public async Task Should_Find_Invitations()
        {
            // Arrange
            LoginAsDefaultTenantAdmin();
            var currentTenant = await GetCurrentTenantAsync();
            await _tenantManager.SetSubscriptionSeats(currentTenant, 1000);
            var input = new CreateUserInvitationDto
            {
                Email = "test@example.com"
            };
            var invitation = await _userInvitationService.CreateInvitationAsync(input);

            // Act
            var getAllInput = new GetAllUserInvitationsInput() { Keyword = "test"};
            var result = await _userInvitationService.GetAll(getAllInput);

            // Assert
            result.ShouldNotBeNull();
            result.TotalCount.ShouldBeGreaterThan(0);
            result.Items.ShouldContain(i => i.Email == invitation.Email);
        }

        [Fact]
        public async Task Should_Create_Multiple_Invitations_Successfully()
        {
            // Arrange
            LoginAsDefaultTenantAdmin();
            var currentTenant = await GetCurrentTenantAsync();
            await _tenantManager.SetSubscriptionSeats(currentTenant, 1000);
            var input = new List<CreateUserInvitationDto>
            {
                new CreateUserInvitationDto { Email = "test1@example.com" },
                new CreateUserInvitationDto { Email = "test2@example.com" }
            };

            // Act
            var result = await _userInvitationService.CreateInvitationsAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.Invitations.Count.ShouldBe(2);
            result.Errors.Count.ShouldBe(0);
            result.Invitations.ShouldContain(i => i.Email == "test1@example.com");
            result.Invitations.ShouldContain(i => i.Email == "test2@example.com");
        }

        [Fact]
        public async Task Should_Handle_Subscription_Limit_For_Multiple_Invitations()
        {
            // Arrange
            LoginAsDefaultTenantAdmin();
            var currentTenant = await GetCurrentTenantAsync();
            await _tenantManager.SetSubscriptionSeats(currentTenant, 1);
            var input = new List<CreateUserInvitationDto>
            {
                new CreateUserInvitationDto { Email = "test1@example.com" },
                new CreateUserInvitationDto { Email = "test2@example.com" }
            };

            // Act
            var result = await _userInvitationService.CreateInvitationsAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.Invitations.Count.ShouldBe(0);
            result.Errors.Count.ShouldBe(1);
            result.Errors[0].ShouldContain("Subscription limit reached");
        }

        [Fact]
        public async Task Should_Handle_Duplicate_Emails_In_Single_Request()
        {
            // Arrange
            LoginAsDefaultTenantAdmin();
            var currentTenant = await GetCurrentTenantAsync();
            await _tenantManager.SetSubscriptionSeats(currentTenant, 1000);
            var input = new List<CreateUserInvitationDto>
            {
                new CreateUserInvitationDto { Email = "duplicate@example.com" },
                new CreateUserInvitationDto { Email = "duplicate@example.com" }
            };

            // Act
            var result = await _userInvitationService.CreateInvitationsAsync(input);

            // Assert
            result.ShouldNotBeNull();
            result.Invitations.Count.ShouldBe(1);
            result.Errors.Count.ShouldBe(0);
            result.Invitations[0].Email.ShouldBe("duplicate@example.com");
        }
    }
}
