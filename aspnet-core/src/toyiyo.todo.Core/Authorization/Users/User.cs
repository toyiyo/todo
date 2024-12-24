using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Abp.Authorization.Users;
using Abp.Extensions;
using Microsoft.EntityFrameworkCore;

namespace toyiyo.todo.Authorization.Users
{
    [Index(nameof(TenantId))]
    public class User : AbpUser<User>
    {
        public readonly string DefaultPassword = (Environment.GetEnvironmentVariable("DefaultPassword") == null ? "" : Environment.GetEnvironmentVariable("DefaultPassword"));

        public static string CreateRandomPassword()
        {
            return Guid.NewGuid().ToString("N").Truncate(16);
        }

        public static User CreateTenantAdminUser(int tenantId, string emailAddress)
        {
            var user = new User
            {
                TenantId = tenantId,
                UserName = AdminUserName,
                Name = AdminUserName,
                Surname = AdminUserName,
                EmailAddress = emailAddress,
                Roles = new List<UserRole>()
            };

            user.SetNormalizedNames();

            return user;
        }

        public static bool ValidateTenantDomainMatchesAdminEmailDomain(string tenancyName, string emailAddress) 
        {
            if (!emailAddress.Contains(tenancyName)){
                throw new ArgumentException("Email must match the domain registered");
            } 
            return true;  
        }

        //extention method to get user initial
        public static string GetInitials(string Name, string Surname)
        {
            return (Name?.Length > 0 ? Name.Substring(0, 1).ToUpper() : "") + (Surname?.Length > 0 ? Surname.Substring(0, 1).ToUpper() : "");
        }
        public static string GetUserColor(string emailAddress)
        {
            return GenerateColorFromString(emailAddress);
        }        

        private static string GenerateColorFromString(string str)
        {
            var colors = new[] {
                "#4CAF50", "#2196F3", "#9C27B0", "#FF9800", "#E91E63",
                "#00BCD4", "#8BC34A", "#FFC107", "#03A9F4", "#FF5722"
            };

            // Generate a consistent hash code using SHA256
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(str));
                var hashInt = BitConverter.ToInt32(hashBytes, 0);
                var index = Math.Abs(hashInt) % colors.Length;
                return colors[index];
            }
        }        
    }
}
