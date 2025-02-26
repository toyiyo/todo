using Abp.Authorization;
using Abp.Localization;
using toyiyo.todo.Authorization;

namespace toyiyo.todo.Core.Authorization
{
    public class TodoAuthorizationProvider : AuthorizationProvider
    {
        public override void SetPermissions(IPermissionDefinitionContext context)
        {
            // ...existing code...

            var administration = context.GetPermissionOrNull(PermissionNames.Pages_Administration_HangfireDashboard)
                ?? context.CreatePermission(PermissionNames.Pages_Administration_HangfireDashboard);

            administration.CreateChildPermission(
                PermissionNames.Pages_Administration_HangfireDashboard,
                L("HangfireDashboard")
            );
        }
        private static ILocalizableString L(string name)
        {
            return new LocalizableString(name, "todo");
        }
    }
}
