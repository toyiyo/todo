using System.Collections.Generic;
using toyiyo.todo.Roles.Dto;

namespace toyiyo.todo.Web.Models.Common
{
    public interface IPermissionsEditViewModel
    {
        List<FlatPermissionDto> Permissions { get; set; }
    }
}