using System.Threading.Tasks;
using Shouldly;
using Xunit;
using toyiyo.todo.Users;
using Abp.UI;
using toyiyo.todo.Authorization.Users;

namespace toyiyo.todo.Tests.Users
{
    public class UserInvitationServiceTests : todoTestBase
    {
        private readonly IUserInvitationAppService _userInvitationService;
        
        public UserInvitationServiceTests()
        {
            // Register IUserInvitationService
            _userInvitationService = Resolve<IUserInvitationAppService>();
        }

        [Fact]
        public async Task Should_Create_Invitation_Successfully()
        {
            // Arrange
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
            await Should.ThrowAsync<UserFriendlyException>(async () =>
            {
                await _userInvitationService.CreateInvitationAsync(input);
            });
        }
    }
}
