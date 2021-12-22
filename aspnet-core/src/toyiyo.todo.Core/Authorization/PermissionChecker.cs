using Abp.Authorization;
using toyiyo.todo.Authorization.Roles;
using toyiyo.todo.Authorization.Users;

namespace toyiyo.todo.Authorization
{
    public class PermissionChecker : PermissionChecker<Role, User>
    {
        public PermissionChecker(UserManager userManager)
            : base(userManager)
        {
        }
    }
}
