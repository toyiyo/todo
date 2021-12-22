using Abp.AspNetCore.Mvc.ViewComponents;

namespace toyiyo.todo.Web.Views
{
    public abstract class todoViewComponent : AbpViewComponent
    {
        protected todoViewComponent()
        {
            LocalizationSourceName = todoConsts.LocalizationSourceName;
        }
    }
}
