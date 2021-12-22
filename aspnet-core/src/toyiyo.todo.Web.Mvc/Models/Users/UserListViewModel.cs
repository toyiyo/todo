using System.Collections.Generic;
using toyiyo.todo.Roles.Dto;

namespace toyiyo.todo.Web.Models.Users
{
    public class UserListViewModel
    {
        public IReadOnlyList<RoleDto> Roles { get; set; }
    }
}
