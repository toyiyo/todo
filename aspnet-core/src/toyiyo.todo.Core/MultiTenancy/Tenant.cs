using System.ComponentModel;
using Abp.MultiTenancy;
using toyiyo.todo.Authorization.Users;

namespace toyiyo.todo.MultiTenancy
{
    public class Tenant : AbpTenant<User>
    {
        [DefaultValue("false")]
        public bool AllowsSelfRegistration { get; set; }
        public Tenant()
        {            
        }

        public Tenant(string tenancyName, string name)
            : base(tenancyName, name)
        {
        }
    }
}
