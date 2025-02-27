using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using toyiyo.todo.Notifications.Dto;

namespace toyiyo.todo.Notifications
{
    [AbpAuthorize]
    public class NotificationPreferenceAppService : todoAppServiceBase, INotificationPreferenceAppService
    {
        private readonly INotificationPreferenceManager _notificationPreferenceManager;

        public NotificationPreferenceAppService(INotificationPreferenceManager notificationPreferenceManager)
        {
            _notificationPreferenceManager = notificationPreferenceManager;
        }

        public async Task<NotificationPreferencesDto> GetUserPreferences()
        {
            var userId = (int)AbpSession.UserId.Value;
            var preferences = await _notificationPreferenceManager.GetUserPreferences(userId);

            // Initialize default preferences if none exist
            if (preferences.Count == 0)
            {
                preferences = await InitializeDefaultPreferences(userId);
            }

            var dtos = ObjectMapper.Map<List<NotificationPreferenceDto>>(preferences);

            // Add display names
            foreach (var dto in dtos)
            {
                dto.DisplayName = L($"NotificationType_{dto.NotificationType}");
                dto.ChannelDisplayName = L($"NotificationChannel_{dto.Channel}");
            }

            return new NotificationPreferencesDto(dtos.Count, dtos);
        }

        public async Task<NotificationPreferenceDto> UpdatePreference(UpdateNotificationPreferenceInput input)
        {
            var userId = (int)AbpSession.UserId.Value;
            var preferences = await _notificationPreferenceManager.GetUserPreferences(userId);
            
            var preference = preferences.Find(p => 
                p.NotificationType == input.NotificationType && 
                p.Channel == input.Channel);

            if (preference == null)
            {
                preference = NotificationPreference.Create(userId, input.NotificationType, input.Channel);
                preference.Toggle(input.IsEnabled, userId);
                await _notificationPreferenceManager.Create(preference);
            }
            else
            {
                preference.Toggle(input.IsEnabled, userId);
                await _notificationPreferenceManager.Update(preference);
            }

            return ObjectMapper.Map<NotificationPreferenceDto>(preference);
        }

        private async Task<List<NotificationPreference>> InitializeDefaultPreferences(int userId)
        {
            var preferences = new List<NotificationPreference>();
            
            var emailPref = NotificationPreference.Create(userId, NotificationType.UserMention, NotificationChannel.Email);
            var inAppPref = NotificationPreference.Create(userId, NotificationType.UserMention, NotificationChannel.InApp);

            preferences.Add(await _notificationPreferenceManager.Create(emailPref));
            preferences.Add(await _notificationPreferenceManager.Create(inAppPref));

            return preferences;
        }
    }
}
