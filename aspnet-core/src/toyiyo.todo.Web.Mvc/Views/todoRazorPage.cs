using Abp.AspNetCore.Mvc.Views;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Mvc.Razor.Internal;

namespace toyiyo.todo.Web.Views
{
    public abstract class todoRazorPage<TModel> : AbpRazorPage<TModel>
    {
        [RazorInject]
        public IAbpSession AbpSession { get; set; }

        protected todoRazorPage()
        {
            LocalizationSourceName = todoConsts.LocalizationSourceName;
        }
    }
}
