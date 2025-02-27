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
            var userId = 1;
            var preference = NotificationPreference.Create(1, NotificationType.UserMention, NotificationChannel.Email);
            
            // Act & Assert
            preference.Toggle(false, userId);
            preference.IsEnabled.ShouldBeFalse();
            preference.LastModifierUserId.ShouldBe(userId);

            preference.Toggle(true, userId);
            preference.IsEnabled.ShouldBeTrue();
            preference.LastModifierUserId.ShouldBe(userId);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Should_Not_Create_With_Invalid_UserId(int invalidUserId)
        {
            // Act & Assert
            Should.Throw<ArgumentException>(() =>
                NotificationPreference.Create(invalidUserId, NotificationType.UserMention, NotificationChannel.Email)
            );
        }

        [Fact]
        public void Should_Not_Toggle_With_Invalid_UserId()
        {
            // Arrange
            var preference = NotificationPreference.Create(1, NotificationType.UserMention, NotificationChannel.Email);

            // Act & Assert
            Should.Throw<ArgumentException>(() =>
                preference.Toggle(false, 0)
            );
        }
    }
}
