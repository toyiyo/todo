using System.ComponentModel;
using Abp.MultiTenancy;
using toyiyo.todo.Authorization.Users;

namespace toyiyo.todo.MultiTenancy
{
    public class Tenant : AbpTenant<User>
    {
        [DefaultValue("true")]
        public bool AllowsSelfRegistration { get; set; }
        public string ExternalSubscriptionId { get; protected set; }
        public int SubscriptionSeats { get; protected set; }
        public Tenant()
        {            
        }

        public Tenant(string tenancyName, string name)
            : base(tenancyName, name)
        {
        }

        public Tenant SetExternalSubscriptionId(string externalSubscriptionId)
        {
            ExternalSubscriptionId = externalSubscriptionId;
            return this;
        }
        public Tenant SetSubscriptionSeats(int subscriptionSeats)
        {
            SubscriptionSeats = subscriptionSeats;
            return this;
        }
    }
}
