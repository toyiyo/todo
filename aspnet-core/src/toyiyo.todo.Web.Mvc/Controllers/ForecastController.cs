using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using toyiyo.todo.Controllers;
using toyiyo.todo.Forecasting;
using toyiyo.todo.Projects;
using toyiyo.todo.Jobs;

namespace toyiyo.todo.Web.Mvc.Controllers
{
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

        //GET: /project/{id}/forecast
        [HttpGet]
        [Route("projects/{projectId}/forecast")]
        public async Task<IActionResult> Index(Guid projectId)
        {
            var project = await _projectAppService.Get(projectId);
            return View(project);
        }

        [HttpGet]
        [Route("projects/{projectId}/forecast/{level}")]
        public async Task<JsonResult> GetForecast(Guid projectId, Job.JobLevel level)
        {
            var forecast = await _forecastAppService.GetForecast(projectId, level);
            return Json(forecast);
        }
    }
}
