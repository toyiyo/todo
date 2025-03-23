using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using toyiyo.todo.Controllers;
using toyiyo.todo.Forecasting;
using toyiyo.todo.Projects;
using toyiyo.todo.Jobs;

namespace toyiyo.todo.Web.Controllers
{
    [Route("projects")]
    public class ForecastController : todoControllerBase
    {
        private readonly IForecastAppService _forecastAppService;
        private readonly IProjectAppService _projectAppService;

        public ForecastController(
            IForecastAppService forecastAppService,
            IProjectAppService projectAppService)
        {
            _forecastAppService = forecastAppService;
            _projectAppService = projectAppService;
        }

        [HttpGet]
        [Route("{projectId}/forecast")]
        public async Task<IActionResult> Index(Guid projectId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var project = await _projectAppService.Get(projectId);
                if (project == null)
                {
                    return NotFound();
                }
                return View(project);
            }
            catch (Exception ex)
            {
                Logger.Error("Error retrieving project forecast", ex);
                return BadRequest("Failed to retrieve project forecast");
            }
        }

        [HttpGet]
        [Route("{projectId}/forecast/{level}")]
        public async Task<IActionResult> GetForecast(Guid projectId, Job.JobLevel level)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (!Enum.IsDefined(typeof(Job.JobLevel), level))
            {
                return BadRequest("Invalid job level specified");
            }

            try
            {
                var forecast = await _forecastAppService.GetForecast(projectId, level);
                return Json(forecast);
            }
            catch (Exception ex)
            {
                Logger.Error("Error getting forecast data", ex);
                return BadRequest(new { error = "Failed to retrieve forecast data", details = ex.Message });
            }
        }
    }
}
