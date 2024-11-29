using System.Threading.Tasks;
using Shouldly;
using Xunit;
using toyiyo.todo.Users;
using Abp.UI;
using toyiyo.todo.Authorization.Users;
using toyiyo.todo.MultiTenancy;
using toyiyo.todo.Invitations;
using toyiyo.todo.Invitations.Dto;

namespace toyiyo.todo.Tests.Users
{
    public class UserInvitationServiceTests : todoTestBase
    {
        private readonly IUserInvitationAppService _userInvitationService;
        private readonly TenantManager _tenantManager;

        public UserInvitationServiceTests()
        {
            // Register IUserInvitationService
            _userInvitationService = Resolve<IUserInvitationAppService>();
            _tenantManager = Resolve<TenantManager>();
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

            await Should.ThrowAsync<UserFriendlyException>(async () =>
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

            await Should.ThrowAsync<UserFriendlyException>(async () =>
            {
                await _userInvitationService.CreateInvitationAsync(input);
            });
        }

    }
}
