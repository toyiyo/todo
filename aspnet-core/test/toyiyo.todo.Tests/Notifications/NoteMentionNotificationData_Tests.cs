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
            var noteId = Guid.NewGuid();

            // Act
            var notificationData = new NoteMentionNotificationData(senderUsername, jobTitle, noteContent, jobId, noteId);

            // Assert
            notificationData.SenderUsername.ShouldBe(senderUsername);
            notificationData.JobTitle.ShouldBe(jobTitle);
            notificationData.NoteContent.ShouldBe(noteContent);
            notificationData.JobId.ShouldBe(jobId);
            notificationData.NoteId.ShouldBe(noteId);
        }
    }
}
