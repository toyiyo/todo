using System.Collections.Generic;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace toyiyo.todo.Notifications.Dto
{
    [AutoMap(typeof(NotificationPreference))]
    public class NotificationPreferenceDto : EntityDto<long>
    {
        public NotificationType NotificationType { get; set; }
        public NotificationChannel Channel { get; set; }
        public bool IsEnabled { get; set; }
        public string DisplayName { get; set; }
        public string ChannelDisplayName { get; set; }
    }

    public class UpdateNotificationPreferenceInput
    {
        public NotificationType NotificationType { get; set; }
        public NotificationChannel Channel { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class NotificationPreferencesDto : PagedResultDto<NotificationPreferenceDto>
    {
        public NotificationPreferencesDto(int totalCount, IReadOnlyList<NotificationPreferenceDto> items) 
            : base(totalCount, items) { }
    }
}
