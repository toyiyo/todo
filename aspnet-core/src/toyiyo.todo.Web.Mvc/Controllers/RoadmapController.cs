using Abp.AspNetCore.Mvc.Authorization;
using Abp.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace toyiyo.todo.Web.Controllers
{
    [AbpMvcAuthorize]
    public class RoadmapController : AbpController
    {
        public IActionResult Index()
        {
            return View();
        }

    }
}
