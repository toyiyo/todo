using Hangfire.Dashboard;
using Hangfire.Annotations;
using toyiyo.todo.Authorization.Roles;

namespace toyiyo.todo.Web.HangFire.Authorization
{
    public class AbpHangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            return httpContext.User.Identity.IsAuthenticated && 
                   httpContext.User.IsInRole(StaticRoleNames.Host.Admin);
        }
    }
}