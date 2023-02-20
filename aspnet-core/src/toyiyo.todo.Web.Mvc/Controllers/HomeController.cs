using Microsoft.AspNetCore.Mvc;
using Abp.AspNetCore.Mvc.Authorization;
using toyiyo.todo.Controllers;
using toyiyo.todo.Jobs;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace toyiyo.todo.Web.Controllers
{
    [AbpMvcAuthorize]
    public class HomeController : todoControllerBase
    {
        private IJobAppService _jobAppService;

        public HomeController(IJobAppService jobAppService)
        {
            _jobAppService = jobAppService;
        }

        public async Task<ActionResult> IndexAsync()
        {
            var jobStats = await _jobAppService.GetJobStats(new GetAllJobsInput(){ MaxResultCount = int.MaxValue });
            var jsonTotalCompletedJobsPerMonth= JsonConvert.SerializeObject(jobStats.TotalCompletedJobsPerMonth);
            ViewBag.TotalCompletedJobsPerMonth = jsonTotalCompletedJobsPerMonth;
            return View(jobStats);
        }
    }
}
