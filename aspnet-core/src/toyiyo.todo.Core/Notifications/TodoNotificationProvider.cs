using Abp.Localization;
using Abp.Notifications;

namespace toyiyo.todo.Notifications
{
    public class TodoNotificationProvider : NotificationProvider
    {
        public override void SetNotifications(INotificationDefinitionContext context)
        {
            context.Manager.Add(
                new NotificationDefinition(
                    NotificationTypes.UserMentioned,
                    displayName: new LocalizableString("UserMentionedNotification", "todo"),
                    permissionDependency: null
                )
            );

            context.Manager.Add(
                new NotificationDefinition(
                    NotificationTypes.NoteReply,
                    displayName: new LocalizableString("NoteReplyNotification", "todo"),
                    permissionDependency: null
                )
            );
        }
    }
}
