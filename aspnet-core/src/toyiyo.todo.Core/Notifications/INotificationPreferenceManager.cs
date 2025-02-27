using System.Collections.Generic;
using System.Threading.Tasks;

namespace toyiyo.todo.Notifications
{
    public interface INotificationPreferenceManager
    {
        Task<NotificationPreference> Create(NotificationPreference preference);
        Task<NotificationPreference> Update(NotificationPreference preference);
        Task<List<NotificationPreference>> GetUserPreferences(int userId);
    }
}
