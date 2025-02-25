using System;
using Shouldly;
using toyiyo.todo.Notifications;
using Xunit;

namespace toyiyo.todo.Tests.Notifications
{
    public class NotificationPreference_Tests : todoTestBase
    {
        [Fact]
        public void Should_Create_Valid_NotificationPreference()
        {
            // Arrange
            var userId = 1;
            var notificationType = NotificationType.UserMention;
            var channel = NotificationChannel.Email;

            // Act
            var preference = NotificationPreference.Create(userId, notificationType, channel);

            // Assert
            preference.ShouldNotBeNull();
            preference.UserId.ShouldBe(userId);
            preference.NotificationType.ShouldBe(notificationType);
            preference.Channel.ShouldBe(channel);
            preference.IsEnabled.ShouldBeTrue(); // Default should be enabled
        }

        [Fact]
        public void Should_Toggle_Preference()
        {
            // Arrange
            var preference = NotificationPreference.Create(1, NotificationType.UserMention, NotificationChannel.Email);
            
            // Act
            preference.Toggle(false, 1);

            // Assert
            preference.IsEnabled.ShouldBeFalse();
        }

        [Fact]
        public void Should_Not_Create_With_Invalid_UserId()
        {
            // Act & Assert
            Should.Throw<ArgumentException>(() =>
                NotificationPreference.Create(0, NotificationType.UserMention, NotificationChannel.Email)
            );
        }
    }
}
