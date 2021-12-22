using System.Threading.Tasks;
using Abp.Application.Services;
using toyiyo.todo.Authorization.Accounts.Dto;

namespace toyiyo.todo.Authorization.Accounts
{
    public interface IAccountAppService : IApplicationService
    {
        Task<IsTenantAvailableOutput> IsTenantAvailable(IsTenantAvailableInput input);

        Task<RegisterOutput> Register(RegisterInput input);
    }
}
