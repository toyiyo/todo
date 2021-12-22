using Abp.Application.Services;
using toyiyo.todo.MultiTenancy.Dto;

namespace toyiyo.todo.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
    }
}

