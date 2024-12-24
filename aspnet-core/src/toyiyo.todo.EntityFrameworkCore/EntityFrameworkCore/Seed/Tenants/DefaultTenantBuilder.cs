using System.Linq;
using Microsoft.EntityFrameworkCore;
using Abp.MultiTenancy;
using toyiyo.todo.Editions;
using toyiyo.todo.MultiTenancy;

namespace toyiyo.todo.EntityFrameworkCore.Seed.Tenants
{
    public class DefaultTenantBuilder
    {
        private readonly todoDbContext _context;

        public DefaultTenantBuilder(todoDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            CreateDefaultTenant();
            CreateTestTenant();
        }

        private void CreateDefaultTenant()
        {
            // Default tenant

            var defaultTenant = _context.Tenants.IgnoreQueryFilters().FirstOrDefault(t => t.TenancyName == AbpTenantBase.DefaultTenantName);
            if (defaultTenant == null)
            {
                defaultTenant = new Tenant(AbpTenantBase.DefaultTenantName, AbpTenantBase.DefaultTenantName);

                var defaultEdition = _context.Editions.IgnoreQueryFilters().FirstOrDefault(e => e.Name == EditionManager.DefaultEditionName);
                if (defaultEdition != null)
                {
                    defaultTenant.EditionId = defaultEdition.Id;
                }

                _context.Tenants.Add(defaultTenant);
                _context.SaveChanges();
            }
        }

        private void CreateTestTenant()
        {
            // Test tenant

            var testTenant = _context.Tenants.IgnoreQueryFilters().FirstOrDefault(t => t.TenancyName == "Test");
            if (testTenant == null)
            {
                testTenant = new Tenant("Test", "Test");

                var defaultEdition = _context.Editions.IgnoreQueryFilters().FirstOrDefault(e => e.Name == EditionManager.DefaultEditionName);
                if (defaultEdition != null)
                {
                    testTenant.EditionId = defaultEdition.Id;
                }

                _context.Tenants.Add(testTenant);
                _context.SaveChanges();
            }
        }
    }
}
