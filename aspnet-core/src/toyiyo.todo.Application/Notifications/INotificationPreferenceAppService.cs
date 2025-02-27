using System.Threading.Tasks;
using Abp.Application.Services;
using toyiyo.todo.Notifications.Dto;

namespace toyiyo.todo.Notifications
{
    public interface INotificationPreferenceAppService : IApplicationService
    {
        Task<NotificationPreferencesDto> GetUserPreferences();
        Task<NotificationPreferenceDto> UpdatePreference(UpdateNotificationPreferenceInput input);
    }
}
