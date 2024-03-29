﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Abp.Application.Services.Dto;
using Abp.AspNetCore.Mvc.Authorization;
using toyiyo.todo.Authorization;
using toyiyo.todo.Controllers;
using toyiyo.todo.Users;
using toyiyo.todo.Web.Models.Users;

namespace toyiyo.todo.Web.Controllers
{
    [AbpMvcAuthorize]
    public class UsersController : todoControllerBase
    {
        private readonly IUserAppService _userAppService;

        public UsersController(IUserAppService userAppService)
        {
            _userAppService = userAppService;
        }
        [AbpMvcAuthorize(PermissionNames.Pages_Users)]
        public async Task<ActionResult> Index()
        {
            var roles = (await _userAppService.GetRoles()).Items;
            var model = new UserListViewModel
            {
                Roles = roles
            };
            return View(model);
        }
        [AbpMvcAuthorize(PermissionNames.Pages_Users)]
        public async Task<ActionResult> EditModal(long userId)
        {
            var user = await _userAppService.GetAsync(new EntityDto<long>(userId));
            var roles = (await _userAppService.GetRoles()).Items;
            var model = new EditUserModalViewModel
            {
                User = user,
                Roles = roles
            };
            return PartialView("_EditModal", model);
        }

        [AbpMvcAuthorize(PermissionNames.Pages_Users_PasswordChange)]
        public ActionResult ChangePassword()
        {
            return View();
        }
    }
}
