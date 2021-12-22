using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using toyiyo.todo.Roles.Dto;
using toyiyo.todo.Users.Dto;

namespace toyiyo.todo.Users
{
    public interface IUserAppService : IAsyncCrudAppService<UserDto, long, PagedUserResultRequestDto, CreateUserDto, UserDto>
    {
        Task DeActivate(EntityDto<long> user);
        Task Activate(EntityDto<long> user);
        Task<ListResultDto<RoleDto>> GetRoles();
        Task ChangeLanguage(ChangeUserLanguageDto input);

        Task<bool> ChangePassword(ChangePasswordDto input);
    }
}
