using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc;
using toyiyo.todo.Authorization;
using toyiyo.todo.Controllers;
using toyiyo.todo.Invitations;
using System.Collections.Generic;
using toyiyo.todo.Invitations.Dto;

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

        [HttpPost]
        public async Task<JsonResult> CreateInvitations([FromBody] List<CreateUserInvitationDto> input)
        {
            var result = await _userInvitationAppService.CreateInvitationsAsync(input);
            return Json(new { result.Invitations, result.Errors });
        }
    }
}