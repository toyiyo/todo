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
            var hash = 0;
            foreach (var c in str)
            {
                hash = c + ((hash << 5) - hash);
            }
            var color = "#";
            for (var i = 0; i < 3; i++)
            {
                var value = (hash >> (i * 8)) & 0xFF;
                color += value.ToString("X2");
            }
            return color;
        }
    }
}

