using Microsoft.AspNetCore.Mvc;
using Abp.AspNetCore.Mvc.Authorization;
using toyiyo.todo.Controllers;
using toyiyo.todo.Authorization;
using toyiyo.todo.Jobs;
using System.Threading.Tasks;
using System;
using toyiyo.todo.Web.Models.Jobs;

namespace toyiyo.todo.Web.Controllers
{
    [AbpMvcAuthorize]
    public class JobsController : todoControllerBase
    {
        public JobsController(IJobAppService JobAppService)
        {
            JobAppService = JobAppService;
        }

        public IJobAppService JobAppService { get; }


        [HttpGet("/projects/{id}/jobs")]
        public async Task<IActionResult> Index(Guid id)
        {
            //var output = await JobAppService.GetAll(new EntityDto(id));
            return View();
        }

        public async Task<IActionResult> EditModal(Guid JobId)
        {
            var output = await JobAppService.Get(JobId);
            var model = ObjectMapper.Map<EditJobModalViewModel>(output);

            return PartialView("_EditModal", model);
        }
    }
}