using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Services;

namespace toyiyo.todo.Notifications
{
    public class NotificationPreferenceManager : DomainService, INotificationPreferenceManager
    {
        private readonly IRepository<NotificationPreference, long> _preferenceRepository;

        public NotificationPreferenceManager(IRepository<NotificationPreference, long> preferenceRepository)
        {
            _preferenceRepository = preferenceRepository;
        }

        public async Task<NotificationPreference> Create(NotificationPreference preference)
        {
            return await _preferenceRepository.InsertAsync(preference);
        }

        public async Task<NotificationPreference> Update(NotificationPreference preference)
        {
            return await _preferenceRepository.UpdateAsync(preference);
        }

        public async Task<List<NotificationPreference>> GetUserPreferences(int userId)
        {
            return await _preferenceRepository.GetAllListAsync(p => p.UserId == userId);
        }
    }
}
