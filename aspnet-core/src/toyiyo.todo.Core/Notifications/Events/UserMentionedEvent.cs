using System;
using Abp;
using Abp.Events.Bus;

namespace toyiyo.todo.Notifications.Events
{
    public class UserMentionedEvent : EventData
    {
        public string MentionedEmail { get; }
        public long MentionedUserId { get; }
        public string MentionedByEmail { get; }
        public long MentionedByUserId { get; }
        public string JobTitle { get; }
        public string Content { get; }
        public Guid JobId { get; }

        public UserMentionedEvent(string mentionedEmail, long mentionedUserId, string mentionedByUsername, long mentionedByUserId, string jobTitle, string content, Guid jobId)
        {
            MentionedEmail = mentionedEmail;
            MentionedUserId = mentionedUserId;
            MentionedByEmail = mentionedByUsername;
            MentionedByUserId = mentionedByUserId;
            JobTitle = jobTitle;
            Content = content;
            JobId = jobId;
        }
    }
}
