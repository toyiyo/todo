using System;
using Abp.Notifications;

namespace toyiyo.todo.Notifications.NotificationData
{
    [Serializable]
    public class NoteMentionNotificationData : MessageNotificationData
    {
        public string SenderUsername { get; set; }
        public string JobTitle { get; set; }
        public string NoteContent { get; set; }
        public Guid JobId { get; set; }
        public Guid NoteId { get; set; }

        public NoteMentionNotificationData(string senderUsername, string jobTitle, string noteContent, Guid jobId) 
            : base(noteContent)
        {
            SenderUsername = senderUsername;
            JobTitle = jobTitle;
            NoteContent = noteContent;
            JobId = jobId;
        }
    }
}
