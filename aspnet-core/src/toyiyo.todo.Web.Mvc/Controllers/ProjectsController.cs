using Microsoft.AspNetCore.Mvc;
using Abp.AspNetCore.Mvc.Authorization;
using toyiyo.todo.Controllers;
using toyiyo.todo.Authorization;
using toyiyo.todo.Projects;
using System.Threading.Tasks;
using System;
using toyiyo.todo.Web.Models.Projects;

namespace toyiyo.todo.Web.Controllers
{
    [AbpMvcAuthorize]
    public class ProjectsController : todoControllerBase
    {
        public ProjectsController(IProjectAppService projectAppService)
        {
            ProjectAppService = projectAppService;
        }

        public IProjectAppService ProjectAppService { get; }



        public async Task<IActionResult> Index() => View();

        public async Task<IActionResult> EditModal(Guid projectId)
        {
            var output = await ProjectAppService.Get(projectId);
            var model = ObjectMapper.Map<EditProjectModalViewModel>(output);

            return PartialView("_EditModal", model);
        }
    }
}