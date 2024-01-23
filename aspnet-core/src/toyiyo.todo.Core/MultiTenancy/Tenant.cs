using System.ComponentModel;
using Abp.MultiTenancy;
using toyiyo.todo.Authorization.Users;

namespace toyiyo.todo.MultiTenancy
{
    public class Tenant : AbpTenant<User>
    {
        [DefaultValue("true")]
        public bool AllowsSelfRegistration { get; set; }
        //todo: at the moment, we are getting this info from stripe, but we should store it in the database via webhooks.
        //this should belong in the subscription class, but we are not using it yet.
        public string ExternalSubscriptionId { get; protected set; }
        public int SubscriptionSeats { get; protected set; }
        public Tenant()
        {            
        }

        public Tenant(string tenancyName, string name)
            : base(tenancyName, name)
        {
        }
    }
}
