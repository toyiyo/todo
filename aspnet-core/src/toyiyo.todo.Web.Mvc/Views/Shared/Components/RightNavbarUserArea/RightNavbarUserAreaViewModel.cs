using System;
using toyiyo.todo.Sessions.Dto;

namespace toyiyo.todo.Web.Views.Shared.Components.RightNavbarUserArea
{
    public class RightNavbarUserAreaViewModel
    {
        public GetCurrentLoginInformationsOutput LoginInformations { get; set; }

        public bool IsMultiTenancyEnabled { get; set; }

        public string GetShownLoginName()
        {
            var userName = LoginInformations.User.UserName;

            if (!IsMultiTenancyEnabled)
            {
                return userName;
            }

            return LoginInformations.Tenant == null
                ? ".\\" + userName
                : LoginInformations.Tenant.TenancyName + "\\" + userName;
        }

        public string GetUserInitials()
        {
            var initials = LoginInformations.User.Name.Substring(0, 1).ToUpper() + LoginInformations.User.Surname.Substring(0, 1).ToUpper();
            return initials;
        }

        public string GetUserColor()
        {
            return GenerateColorFromString(LoginInformations.User.EmailAddress);
        }

        private string GenerateColorFromString(string str)
        {
            // Use a predefined set of accessible colors
            var colors = new[] {
                "#4CAF50", "#2196F3", "#9C27B0", "#FF9800", "#E91E63",
                "#00BCD4", "#8BC34A", "#FFC107", "#03A9F4", "#FF5722"
            };
            
            // Generate a consistent index based on the string
            var index = Math.Abs(str.GetHashCode()) % colors.Length;
            return colors[index];
        }
    }
}

