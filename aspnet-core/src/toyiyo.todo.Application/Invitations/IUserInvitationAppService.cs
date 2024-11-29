using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using toyiyo.todo.Invitations.Dto;

namespace toyiyo.todo.Invitations
{
    public interface IUserInvitationAppService
    {
        public Task<UserInvitationDto> CreateInvitationAsync(CreateUserInvitationDto input);
        public Task<PagedResultDto<UserInvitationDto>> GetAll(GetAllUserInvitationsInput input);
    }
}
