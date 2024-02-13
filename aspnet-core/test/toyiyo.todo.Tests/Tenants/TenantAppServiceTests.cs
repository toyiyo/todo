using System.Threading.Tasks;
using Shouldly;
using toyiyo.todo.Projects;
using toyiyo.todo.Sessions;
using Xunit;
using toyiyo.todo.MultiTenancy;
using toyiyo.todo.MultiTenancy.Dto;
using Abp.ObjectMapping;
using AutoMapper;

namespace toyiyo.todo.Tests.Tenants
{
    public class TenantAppServiceTests : todoTestBase
    {
        private readonly IProjectAppService _projectAppService;
        private readonly ISessionAppService _sessionAppService;
        private readonly ITenantAppService _tenantAppService;
        public TenantAppServiceTests()
        {
            _projectAppService = Resolve<IProjectAppService>();
            _sessionAppService = Resolve<ISessionAppService>();
            _tenantAppService = Resolve<ITenantAppService>();
        }
        [Fact]
        public async Task CreateTenant_ReturnsNewTenant()
        {   
            // Arrange
            LoginAsHostAdmin();
            var createTenantDto = new CreateTenantDto
            {
                TenancyName = "testtenant",
                Name = "test tenant",
                AdminEmailAddress = "test@admin.com",
                Password = "test1234"
            };

            // Act
            var tenant = await _tenantAppService.CreateAsync(createTenantDto);

            // Assert
            tenant.ShouldNotBeNull();
            tenant.Name.ShouldBe(createTenantDto.Name);
            tenant.IsActive.ShouldBe(createTenantDto.IsActive);
            tenant.AllowsSelfRegistration.ShouldBe(true);
        }

        [Fact]
        public void UpdateTenant_SetSelfRegisration_ReturnsValue()
        {
            // Arrange
            //authenticate as administrator
            LoginAsHostAdmin();
            var createTenantDto = new CreateTenantDto
            {
                TenancyName = "testtenant",
                Name = "test tenant",
                AdminEmailAddress = "test@admin.com",
                Password = "test1234"
            };
            var tenant = _tenantAppService.CreateAsync(createTenantDto).Result;
            var tenantDto = new TenantDto
            {
                TenancyName = tenant.TenancyName,
                Id = tenant.Id,
                Name = tenant.Name,
                IsActive = tenant.IsActive,
                AllowsSelfRegistration = true
            };

            // Act
            var updatedTenant = _tenantAppService.UpdateAsync(tenantDto).Result;

            // Assert
            updatedTenant.ShouldNotBeNull();
            updatedTenant.AllowsSelfRegistration.ShouldBe(true);
        }
    }
}