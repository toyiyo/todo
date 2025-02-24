using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using toyiyo.todo.Controllers;
using toyiyo.todo.Notifications;

namespace toyiyo.todo.Web.Mvc.Controllers
{
    [AbpMvcAuthorize]
    public class NotificationPreferencesController : todoControllerBase
    {
        private readonly INotificationPreferenceAppService _notificationPreferenceAppService;

        public NotificationPreferencesController(INotificationPreferenceAppService notificationPreferenceAppService)
        {
            _notificationPreferenceAppService = notificationPreferenceAppService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _notificationPreferenceAppService.GetUserPreferences();
            return View(model);
        }
    }
}
