using System.Threading.Tasks;
using Abp.Application.Features;
using Abp.Domain.Repositories;
using Abp.MultiTenancy;
using toyiyo.todo.Authorization.Users;
using toyiyo.todo.Editions;

namespace toyiyo.todo.MultiTenancy
{
    public class TenantManager : AbpTenantManager<Tenant, User>
    {
        public TenantManager(
            IRepository<Tenant> tenantRepository, 
            IRepository<TenantFeatureSetting, long> tenantFeatureRepository, 
            EditionManager editionManager,
            IAbpZeroFeatureValueStore featureValueStore) 
            : base(
                tenantRepository, 
                tenantFeatureRepository, 
                editionManager,
                featureValueStore)
        {
        }

        public async Task<Tenant> SetExternalSubscriptionId(Tenant tenant, string externalSubscriptionId) {
            tenant.SetExternalSubscriptionId(externalSubscriptionId);
            await UpdateAsync(tenant);
            return tenant;
        }
    }
}
