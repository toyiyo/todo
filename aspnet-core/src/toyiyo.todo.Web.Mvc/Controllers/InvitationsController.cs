using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using toyiyo.todo.Authorization;
using toyiyo.todo.Controllers;
using toyiyo.todo.Invitations;

namespace toyiyo.todo.Web.Controllers
{
    [AbpMvcAuthorize(PermissionNames.Pages_Subscription)]
    public class InvitationsController : todoControllerBase
    {
        private readonly IUserInvitationAppService _userInvitationAppService;

        public InvitationsController(IUserInvitationAppService userInvitationAppService)
        {
            _userInvitationAppService = userInvitationAppService;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }
    }
}