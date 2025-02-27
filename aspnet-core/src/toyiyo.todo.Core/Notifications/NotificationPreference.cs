using System;
using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Abp.Timing;

namespace toyiyo.todo.Notifications
{
    public class NotificationPreference : FullAuditedEntity<long>, IMustHaveTenant
    {
        public int UserId { get; protected set; }
        public NotificationType NotificationType { get; protected set; }
        public NotificationChannel Channel { get; protected set; }
        public bool IsEnabled { get; protected set; }
        public int TenantId { get; set; }

        protected NotificationPreference() { }

        public static NotificationPreference Create(int userId, NotificationType type, NotificationChannel channel)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("UserId must be greater than 0", nameof(userId));
            }

            return new NotificationPreference
            {
                UserId = userId,
                NotificationType = type,
                Channel = channel,
                IsEnabled = true, // Enabled by default
                CreationTime = Clock.Now,
                CreatorUserId = userId,
                LastModificationTime = Clock.Now,
                LastModifierUserId = userId
            };
        }

        public void Toggle(bool enabled, long userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentException("UserId must be greater than 0", nameof(userId));
            }

            if (userId != UserId)
            {
                throw new InvalidOperationException("Cannot toggle preference for another user.");
            }

            IsEnabled = enabled;
            LastModifierUserId = userId;
            LastModificationTime = Clock.Now;
        }
    }
}
