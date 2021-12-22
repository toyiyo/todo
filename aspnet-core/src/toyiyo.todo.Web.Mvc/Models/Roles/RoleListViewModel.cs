using System.Collections.Generic;
using toyiyo.todo.Roles.Dto;

namespace toyiyo.todo.Web.Models.Roles
{
    public class RoleListViewModel
    {
        public IReadOnlyList<PermissionDto> Permissions { get; set; }
    }
}
