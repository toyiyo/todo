using System.Threading.Tasks;
using Shouldly;
using toyiyo.todo.Notifications;
using toyiyo.todo.Notifications.Dto;
using Xunit;

namespace toyiyo.todo.Tests.Notifications
{
    public class NotificationPreferenceAppService_Tests : todoTestBase
    {
        private readonly INotificationPreferenceAppService _notificationPreferenceAppService;

        public NotificationPreferenceAppService_Tests()
        {
            _notificationPreferenceAppService = Resolve<INotificationPreferenceAppService>();
        }

        [Fact]
        public async Task Should_Get_User_Preferences()
        {
            // Arrange
            LoginAsDefaultTenantAdmin();

            // Act
            var preferences = await _notificationPreferenceAppService.GetUserPreferences();

            // Assert
            preferences.ShouldNotBeNull();
            preferences.Items.ShouldNotBeEmpty();
            preferences.Items.ShouldContain(p => p.NotificationType == NotificationType.UserMention);
        }

        [Fact]
        public async Task Should_Update_User_Preference()
        {
            // Arrange
            LoginAsDefaultTenantAdmin();
            var input = new UpdateNotificationPreferenceInput
            {
                NotificationType = NotificationType.UserMention,
                Channel = NotificationChannel.Email,
                IsEnabled = false
            };

            // Act
            var result = await _notificationPreferenceAppService.UpdatePreference(input);

            // Assert
            result.ShouldNotBeNull();
            result.IsEnabled.ShouldBeFalse();

            // Verify the change persisted
            var preferences = await _notificationPreferenceAppService.GetUserPreferences();
            preferences.Items.ShouldContain(p => 
                p.NotificationType == input.NotificationType && 
                p.Channel == input.Channel && 
                p.IsEnabled == input.IsEnabled
            );
        }
    }
}
