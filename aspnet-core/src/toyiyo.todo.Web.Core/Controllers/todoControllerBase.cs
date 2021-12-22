using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace toyiyo.todo.Controllers
{
    public abstract class todoControllerBase: AbpController
    {
        protected todoControllerBase()
        {
            LocalizationSourceName = todoConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
