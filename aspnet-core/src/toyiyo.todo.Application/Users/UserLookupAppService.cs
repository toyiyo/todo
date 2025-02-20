using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using toyiyo.todo.Authorization.Users;
using toyiyo.todo.Users.Dto;

namespace toyiyo.todo.Users
{
    [AbpAuthorize]
    public class UserLookupAppService : todoAppServiceBase, IUserLookupAppService
    {
        private readonly IRepository<User, long> _userRepository;

        public UserLookupAppService(IRepository<User, long> userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<List<UserLookupDto>> SearchUsers(string searchTerm, int maxResults = 5)
        {
            var query = _userRepository.GetAll()
                .Where(u => u.UserName.Contains(searchTerm) || 
                           u.Name.Contains(searchTerm) || 
                           u.Surname.Contains(searchTerm))
                .Take(maxResults);

            var users = await query.ToListAsync();

            return users.Select(u => new UserLookupDto
            {
                Id = u.Id,
                UserName = u.UserName,
                DisplayName = $"{u.Name} {u.Surname}".Trim(),
                EmailAddress = u.EmailAddress
            }).ToList();
        }
    }
}
