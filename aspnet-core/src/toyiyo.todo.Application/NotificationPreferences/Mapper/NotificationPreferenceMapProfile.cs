using AutoMapper;
using toyiyo.todo.Notifications;
using toyiyo.todo.Notifications.Dto;

namespace toyiyo.todo.NotificationPreferences.Mapper
{
    public class NotificationPreferenceMapProfile : Profile
    {
        public NotificationPreferenceMapProfile()
        {
            CreateMap<NotificationPreference, NotificationPreferenceDto>();
        }
    }
}
