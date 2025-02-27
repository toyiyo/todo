using System;
using Abp.Notifications;

namespace toyiyo.todo.Notifications.NotificationData
{
    [Serializable]
    public class NoteMentionNotificationData : MessageNotificationData
    {
        public string SenderUsername { get; set; }
        public string JobTitle { get; set; }
        public string NotificationMessage { get; set; }
        public Guid JobId { get; set; }

        public NoteMentionNotificationData(string senderUsername, string jobTitle, string notificationMessage, Guid jobId) 
            : base(notificationMessage)
        {
            SenderUsername = senderUsername;
            JobTitle = jobTitle;
            NotificationMessage = notificationMessage;
            JobId = jobId;
        }
    }
}
