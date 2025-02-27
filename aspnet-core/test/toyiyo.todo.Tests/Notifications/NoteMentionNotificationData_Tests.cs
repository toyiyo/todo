using System;
using toyiyo.todo.Notifications.NotificationData;
using Shouldly;
using Xunit;

namespace toyiyo.todo.Tests.Notifications
{
    public class NoteMentionNotificationData_Tests: todoTestBase
    {
        [Fact]
        public void Should_Create_NoteMentionNotificationData()
        {
            // Arrange
            var senderUsername = "testuser";
            var jobTitle = "Test Job";
            var noteContent = "This is a test note.";
            var jobId = Guid.NewGuid();

            // Act
            var notificationData = new NoteMentionNotificationData(senderUsername, jobTitle, noteContent, jobId);

            // Assert
            notificationData.SenderUsername.ShouldBe(senderUsername);
            notificationData.JobTitle.ShouldBe(jobTitle);
            notificationData.NotificationMessage.ShouldBe(noteContent);
            notificationData.JobId.ShouldBe(jobId);
        }
    }
}
