using Microsoft.AspNetCore.Mvc;
using Abp.AspNetCore.Mvc.Authorization;
using toyiyo.todo.Controllers;
using toyiyo.todo.Authorization;
using toyiyo.todo.Jobs;
using System.Threading.Tasks;
using System;
using toyiyo.todo.Web.Models.Jobs;
using System.Collections.Generic;

namespace toyiyo.todo.Web.Controllers
{
    [AbpMvcAuthorize]
    public class JobsController : todoControllerBase
    {
        public JobsController(IJobAppService jobAppService)
        {
            JobAppService = jobAppService;
        }

        public IJobAppService JobAppService { get; }


        [HttpGet("/projects/{projectId}/jobs")]
        [HttpGet("/projects/{projectId}/jobs/{jobId}")]
        public async Task<IActionResult> Index(Guid projectId, Guid? jobId)
        {
            ViewBag.ProjectId = projectId;
            ViewBag.JobId = jobId;
            return View();
        }

        public async Task<IActionResult> EditModal(Guid JobId)
        {
            try
            {
                var output = await JobAppService.Get(JobId);
                if (output == null) { return new NotFoundResult(); }
                //todo, update query to get all subtasks rather than actual jobs.  Subtasks should be a new model rather than a job
                var subTasks = await JobAppService.GetAll(new GetAllJobsInput() { ParentJobId = JobId, MaxResultCount = int.MaxValue });
                //convert map all subtasks to a list of editjobmodalviewmodel and add to viewbag
                ViewBag.SubTasks = ObjectMapper.Map<List<EditJobSubTaskModalViewModel>>(subTasks.Items);

                var model = ObjectMapper.Map<EditJobModalViewModel>(output);
                return PartialView("_EditModal", model);
            }
            catch (ArgumentNullException) { return new NotFoundResult(); }
            catch (Abp.Domain.Entities.EntityNotFoundException) { return new NotFoundResult(); }
        }
    }
}