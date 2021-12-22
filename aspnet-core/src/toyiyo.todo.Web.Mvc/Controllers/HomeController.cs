using Microsoft.AspNetCore.Mvc;
using Abp.AspNetCore.Mvc.Authorization;
using toyiyo.todo.Controllers;

namespace toyiyo.todo.Web.Controllers
{
    [AbpMvcAuthorize]
    public class HomeController : todoControllerBase
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
