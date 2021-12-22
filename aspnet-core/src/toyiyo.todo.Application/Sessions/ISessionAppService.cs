using System.Threading.Tasks;
using Abp.Application.Services;
using toyiyo.todo.Sessions.Dto;

namespace toyiyo.todo.Sessions
{
    public interface ISessionAppService : IApplicationService
    {
        Task<GetCurrentLoginInformationsOutput> GetCurrentLoginInformations();
    }
}
