using System;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.MultiTenancy;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Http;

namespace Abp.AspNetCore.MultiTenancy;

public class UrlParameterTenantResolveContributor : ITenantResolveContributor, ITransientDependency
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITenantStore _tenantStore;
    private readonly IAbpSession _abpSession;

    public UrlParameterTenantResolveContributor(
        IHttpContextAccessor httpContextAccessor,
        ITenantStore tenantStore,
        IAbpSession abpSession)
    {
        _httpContextAccessor = httpContextAccessor;
        _tenantStore = tenantStore;
        _abpSession = abpSession;
    }

    public int? ResolveTenantId()
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
                _abpSession.Use(tenant.Id, null);
                return tenant.Id;
            }
        }

        return null;
    }
}