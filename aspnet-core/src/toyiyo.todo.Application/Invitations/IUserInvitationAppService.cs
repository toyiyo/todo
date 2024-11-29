using System.Threading.Tasks;
using toyiyo.todo.Invitations.Dto;

namespace toyiyo.todo.Invitations
{
    public interface IUserInvitationAppService
    {
        public Task<UserInvitationDto> CreateInvitationAsync(CreateUserInvitationDto input);
    }
}
