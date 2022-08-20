using System;
using System.Collections.Generic;
using Abp.Authorization.Users;
using Abp.Extensions;
using toyiyo.todo.Jobs;

namespace toyiyo.todo.Authorization.Users
{
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
    }
}
