using System.Threading.Tasks;
using Abp.Application.Services;
using System.Collections.Generic;
using toyiyo.todo.Users.Dto;

namespace toyiyo.todo.Users
{
    public interface IUserLookupAppService : IApplicationService
    {
        Task<List<UserLookupDto>> SearchUsers(string searchTerm, int maxResults = 5);
    }
}
