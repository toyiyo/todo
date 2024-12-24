using System;
using toyiyo.todo.Sessions.Dto;
using System.Security.Cryptography;
using System.Text;
using toyiyo.todo.Authorization.Users;

namespace toyiyo.todo.Web.Views.Shared.Components.RightNavbarUserArea
{
    public class RightNavbarUserAreaViewModel
    {
        public GetCurrentLoginInformationsOutput LoginInformations { get; set; }

        public bool IsMultiTenancyEnabled { get; set; }

        public string GetShownLoginName()
        {
            return LoginInformations.User.UserName;
        }

        public string GetUserInitials()
        {
            return User.GetInitials(LoginInformations.User.Name, LoginInformations.User.Surname);
        }

        public string GetUserColor()
        {
            return User.GetUserColor(LoginInformations.User.EmailAddress);
        }
    }
}

