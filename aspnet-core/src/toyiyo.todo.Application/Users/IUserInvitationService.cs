using System.Threading.Tasks;

namespace toyiyo.todo.Users
{
    public interface IUserInvitationService
    {
        public Task<UserInvitationDto> CreateInvitationAsync(CreateUserInvitationDto input);
    }
}
