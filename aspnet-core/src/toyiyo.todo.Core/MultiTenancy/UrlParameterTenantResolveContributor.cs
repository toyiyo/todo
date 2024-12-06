using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.MultiTenancy;
using Microsoft.AspNetCore.Http;

namespace Abp.AspNetCore.MultiTenancy;

public class UrlParameterTenantResolveContributor : ITenantResolveContributor, ITransientDependency
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITenantStore _tenantStore;

    public UrlParameterTenantResolveContributor(
        IHttpContextAccessor httpContextAccessor,
        ITenantStore tenantStore)
    {
        _httpContextAccessor = httpContextAccessor;
        _tenantStore = tenantStore;

    }

    public Task<int?> ResolveTenantId()
    {

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return null;
        }

        var tenantName = httpContext.Request.Query["tenant"];

        if (!string.IsNullOrEmpty(tenantName))
        {
            var tenant = _tenantStore.Find(tenantName);
            if (tenant != null)
            {
                return Task.FromResult<int?>(tenant.Id);
            }
        }

        return Task.FromResult<int?>(null);
    }

    int? ITenantResolveContributor.ResolveTenantId()
    {
        return ResolveTenantId().Result;
    }
}