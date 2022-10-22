//controller to handle legal pages
using Microsoft.AspNetCore.Mvc;
using Abp.Authorization;
using Abp.AspNetCore.Mvc.Controllers;

namespace toyiyo.todo.Web.Controllers
{
    [AbpAllowAnonymous]
    public class LegalController : AbpController
    {
        public IActionResult Terms()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Cookies()
        {
            return View();
        }
    }
}