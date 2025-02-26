using System;
using Abp.Events.Bus;

namespace toyiyo.todo.Notifications.Events
{
    public class UserMentionedEvent : EventData
    {
        public long MentionedUserId { get; }
        public string MentionedByUsername { get; }
        public string JobTitle { get; }
        public string NoteContent { get; }
        public Guid JobId { get; }

        public UserMentionedEvent(long mentionedUserId, string mentionedByUsername, string jobTitle, string noteContent, Guid jobId)
        {
            MentionedUserId = mentionedUserId;
            MentionedByUsername = mentionedByUsername;
            JobTitle = jobTitle;
            NoteContent = noteContent;
            JobId = jobId;
        }
    }
}
