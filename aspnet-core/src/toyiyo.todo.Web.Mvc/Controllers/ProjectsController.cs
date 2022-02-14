using Microsoft.AspNetCore.Mvc;
using Abp.AspNetCore.Mvc.Authorization;
using toyiyo.todo.Controllers;
using toyiyo.todo.Authorization;
using toyiyo.todo.Projects;
using System.Threading.Tasks;

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

        public async Task<ActionResult> IndexAsync()
        {
            var input = new GetAllProjectsInput()
            {
                MaxResultCount = 10,
                SkipCount = 0
            };

            var projects = (await ProjectAppService.GetAll(input)).Items;
            //todo: add view model and map to view model
            return View(projects);
        }
    }
}