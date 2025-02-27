using System.Linq;
using System.Threading.Tasks;
using Abp.Runtime.Session;
using Microsoft.EntityFrameworkCore;
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
            LoginAsDefaultTenantAdmin();
        }

        [Fact]
        public async Task Should_Initialize_Default_Preferences()
        {
            // Act
            var result = await _notificationPreferenceAppService.GetUserPreferences();

            // Assert
            result.TotalCount.ShouldBeGreaterThan(0);
            result.Items.ShouldContain(p => p.NotificationType == NotificationType.UserMention 
                && p.Channel == NotificationChannel.Email);
            result.Items.ShouldContain(p => p.NotificationType == NotificationType.UserMention 
                && p.Channel == NotificationChannel.InApp);

            // Verify all items have display names
            result.Items.All(p => !string.IsNullOrEmpty(p.DisplayName)).ShouldBeTrue();
            result.Items.All(p => !string.IsNullOrEmpty(p.ChannelDisplayName)).ShouldBeTrue();
        }

        [Fact]
        public async Task Should_Update_Existing_Preference()
        {
            // Arrange
            await _notificationPreferenceAppService.GetUserPreferences(); // Initialize preferences
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
            result.NotificationType.ShouldBe(input.NotificationType);
            result.Channel.ShouldBe(input.Channel);

            // Verify the change persisted
            var preferences = await _notificationPreferenceAppService.GetUserPreferences();
            preferences.Items.ShouldContain(p => 
                p.NotificationType == input.NotificationType && 
                p.Channel == input.Channel && 
                p.IsEnabled == input.IsEnabled
            );
        }

        [Fact]
        public async Task Should_Create_New_Preference_If_Not_Exists()
        {
            // Arrange
            var input = new UpdateNotificationPreferenceInput
            {
                NotificationType = NotificationType.UserMention,
                Channel = NotificationChannel.InApp,
                IsEnabled = true
            };

            // Act
            var result = await _notificationPreferenceAppService.UpdatePreference(input);

            // Assert
            result.ShouldNotBeNull();
            result.IsEnabled.ShouldBeTrue();
            result.NotificationType.ShouldBe(input.NotificationType);
            result.Channel.ShouldBe(input.Channel);
        }

        [Fact]
        public async Task Should_Return_Cached_Preferences_On_Subsequent_Calls()
        {
            // Act
            var firstResult = await _notificationPreferenceAppService.GetUserPreferences();
            var secondResult = await _notificationPreferenceAppService.GetUserPreferences();

            // Assert
            firstResult.TotalCount.ShouldBe(secondResult.TotalCount);
            firstResult.Items.Count.ShouldBe(secondResult.Items.Count);
        }

        [Fact]
        public async Task Should_Require_Authentication()
        {
            // Arrange
            var abpSession = Resolve<IAbpSession>();
            abpSession.UserId = null;

            // Act & Assert
            await Should.ThrowAsync<Abp.Authorization.AbpAuthorizationException>(async () =>
                await _notificationPreferenceAppService.GetUserPreferences()
            );
        }

        [Fact]
        public async Task Should_Toggle_Multiple_Times()
        {
            // Arrange
            var input = new UpdateNotificationPreferenceInput
            {
                NotificationType = NotificationType.UserMention,
                Channel = NotificationChannel.Email,
                IsEnabled = false
            };

            // Act & Assert
            var result1 = await _notificationPreferenceAppService.UpdatePreference(input);
            result1.IsEnabled.ShouldBeFalse();

            input.IsEnabled = true;
            var result2 = await _notificationPreferenceAppService.UpdatePreference(input);
            result2.IsEnabled.ShouldBeTrue();

            input.IsEnabled = false;
            var result3 = await _notificationPreferenceAppService.UpdatePreference(input);
            result3.IsEnabled.ShouldBeFalse();
        }
    }
}
