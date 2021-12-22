using Abp.AutoMapper;
using toyiyo.todo.Roles.Dto;
using toyiyo.todo.Web.Models.Common;

namespace toyiyo.todo.Web.Models.Roles
{
    [AutoMapFrom(typeof(GetRoleForEditOutput))]
    public class EditRoleModalViewModel : GetRoleForEditOutput, IPermissionsEditViewModel
    {
        public bool HasPermission(FlatPermissionDto permission)
        {
            return GrantedPermissionNames.Contains(permission.Name);
        }
    }
}
