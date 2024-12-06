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

    /// <summary>
    /// Resolves the tenant ID from the URL parameter.
    /// </summary>
    /// <returns>The tenant ID if found; otherwise, null.</returns>
    public async Task<int?> ResolveTenantIdAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return null;
        }

        var tenantName = await GetTenantNameFromQueryAsync(httpContext);
        if (string.IsNullOrEmpty(tenantName))
        {
            return null;
        }

        return await ResolveTenantFromNameAsync(tenantName);
    }

    private static Task<string> GetTenantNameFromQueryAsync(HttpContext httpContext)
    {
        return Task.FromResult(httpContext.Request.Query["tenant"].ToString());
    }

    private async Task<int?> ResolveTenantFromNameAsync(string tenantName)
    {
        var tenant = await Task.FromResult(_tenantStore.Find(tenantName));
        if (tenant == null)
        {
            return null;
        }

        await Task.Run(() => _abpSession.Use(tenant.Id, null));
        return tenant.Id;
    }

    public int? ResolveTenantId()
    {
        try
        {
            // Using GetAwaiter().GetResult() instead of .Result to get better exception handling
            return ResolveTenantIdAsync().GetAwaiter().GetResult();
        }
        catch (Exception)
        {
            // In case of any error, return null as per the interface's expected behavior
            return null;
        }
    }
}