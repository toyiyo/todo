using System.Threading.Tasks;

namespace toyiyo.todo.Users
{
    public interface IUserInvitationAppService
    {
        public Task<UserInvitationDto> CreateInvitationAsync(CreateUserInvitationDto input);
    }
}
