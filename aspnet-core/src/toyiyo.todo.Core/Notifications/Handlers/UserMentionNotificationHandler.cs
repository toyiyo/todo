using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Events.Bus.Handlers;
using Abp.Notifications;
using Abp.Runtime.Session;
using toyiyo.todo.Notifications.Events;
using toyiyo.todo.Notifications.NotificationData;
using toyiyo.todo.Notifications.Jobs;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace toyiyo.todo.Notifications.Handlers
{
    public class UserMentionNotificationHandler :
        IAsyncEventHandler<UserMentionedEvent>,
        ITransientDependency
    {
        private readonly INotificationPublisher _notificationPublisher;
        private readonly IRepository<NotificationPreference, long> _preferenceRepository;
        private readonly IAbpSession _session;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly ILogger<UserMentionNotificationHandler> _logger;

        public UserMentionNotificationHandler(
            INotificationPublisher notificationPublisher,
            IAbpSession session,
            IBackgroundJobManager backgroundJobManager,
            IRepository<NotificationPreference, long> preferenceRepository,
            ILogger<UserMentionNotificationHandler> logger)
        {
            _notificationPublisher = notificationPublisher;
            _session = session;
            _backgroundJobManager = backgroundJobManager;
            _preferenceRepository = preferenceRepository;
            _logger = logger;
        }

        public async Task HandleEventAsync(UserMentionedEvent eventData)
        {
            // Get user's preferences for both channels in one query
            var userPreferences = await _preferenceRepository.GetAllListAsync(p =>
                p.UserId == (int)eventData.MentionedUserId &&
                p.NotificationType == NotificationType.UserMention &&
                (p.Channel == NotificationChannel.Email || p.Channel == NotificationChannel.InApp));

            var emailPref = userPreferences.FirstOrDefault(p => p.Channel == NotificationChannel.Email);
            var inAppPref = userPreferences.FirstOrDefault(p => p.Channel == NotificationChannel.InApp);

            var notificationData = new NoteMentionNotificationData(
                $"@{eventData.MentionedByUsername} mentioned you in job '{eventData.JobTitle}'",
                eventData.JobTitle,
                eventData.MentionedByUsername,
                eventData.JobId
            );

            // Send in-app notification if enabled
            if (inAppPref?.IsEnabled == true)
            {
                await _notificationPublisher.PublishAsync(
                    notificationName: NotificationTypes.UserMentioned,
                    data: notificationData,
                    userIds: new[] { new Abp.UserIdentifier(_session.TenantId, eventData.MentionedUserId) }
                );
            }

            // Send email notification if enabled
            try
            {
                if (emailPref?.IsEnabled == true)
                {
                    await _backgroundJobManager.EnqueueAsync<EmailSendingJob, EmailSendingArgs>(
                        new EmailSendingArgs
                        {
                            UserId = eventData.MentionedUserId,
                            TenantId = _session.TenantId,
                            Subject = $"You were mentioned in a note in {eventData.JobTitle}",
                            Body = $"@{eventData.MentionedByUsername} mentioned you in job '{eventData.JobTitle}'",
                            EmailAddress = eventData.UserEmail // Use email from event
                        }
                    );
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Failed to enqueue email notification job");
                throw;
            }
        }
    }
}