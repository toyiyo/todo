using Microsoft.AspNetCore.Mvc;
using Abp.AspNetCore.Mvc.Authorization;
using toyiyo.todo.Controllers;
using toyiyo.todo.Authorization;
using toyiyo.todo.Jobs;
using System.Threading.Tasks;
using System;
using toyiyo.todo.Web.Models.Jobs;
using System.Collections.Generic;
using static toyiyo.todo.Jobs.Job;
using toyiyo.todo.Projects;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace toyiyo.todo.Web.Controllers
{
    [AbpMvcAuthorize]
    public class JobsController : todoControllerBase
    {
        public JobsController(IJobAppService jobAppService, IProjectAppService projectAppService)
        {
            JobAppService = jobAppService;
            ProjectAppService = projectAppService;
        }

        public IJobAppService JobAppService { get; }
        public IProjectAppService ProjectAppService { get; }


        [HttpGet("/projects/{projectId}/jobs")]
        [HttpGet("/projects/{projectId}/jobs/{jobId}")]
        public async Task<IActionResult> Index(Guid projectId, Guid? jobId)
        {
            var project = await ProjectAppService.Get(projectId);
            ViewBag.ProjectId = projectId;
            ViewBag.JobId = jobId;
            ViewBag.ProjectTitle = project.Title;

            return View();
        }

        public async Task<IActionResult> EditModal(Guid JobId)
        {
            try
            {
                var output = await JobAppService.Get(JobId);
                if (output == null) { return new NotFoundResult(); }
                
                var subTasks = await JobAppService.GetAll(new GetAllJobsInput() { 
                    ParentJobId = JobId, 
                    MaxResultCount = int.MaxValue, 
                    Levels = new List<JobLevel> { JobLevel.SubTask}.ToArray() 
                });

                // Get all epics for the project
                var epics = await JobAppService.GetAll(new GetAllJobsInput() {
                    ProjectId = output.Project.Id,
                    MaxResultCount = int.MaxValue,
                    Levels = new List<JobLevel> { JobLevel.Epic }.ToArray(),
                    JobStatus = Status.Open
                });

                ViewBag.SubTasks = ObjectMapper.Map<List<EditJobSubTaskModalViewModel>>(subTasks.Items);
                ViewBag.Epics = epics.Items.Select(e => new SelectListItem { 
                    Value = e.Id.ToString(),
                    Text = e.Title,
                    Selected = e.Id == output.ParentId
                }).Prepend(new SelectListItem {
                    Value = Guid.Empty.ToString(),
                    Text = "No Parent",
                    Selected = !output.ParentId.HasValue
                });

                var model = ObjectMapper.Map<EditJobModalViewModel>(output);
                return PartialView("_EditModal", model);
            }
            catch (ArgumentNullException) { return new NotFoundResult(); }
            catch (Abp.Domain.Entities.EntityNotFoundException) { return new NotFoundResult(); }
        }
    }
}