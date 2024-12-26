
using System.Collections.Generic;
using toyiyo.todo.Users.Dto;

namespace toyiyo.todo.Web.Views.Shared.Components.UserDropdown
{
    public class UserDropdownViewModel
    {
        public List<UserDto> Users { get; set; }
        public long? SelectedUserId { get; set; }
        public System.Guid JobId { get; set; }
    }
}