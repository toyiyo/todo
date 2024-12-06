using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using toyiyo.todo.Invitations.Dto;

namespace toyiyo.todo.Invitations
{
    public interface IUserInvitationAppService : IApplicationService
    {
        Task<PagedResultDto<UserInvitationDto>> GetAll(GetAllUserInvitationsInput input);
        Task<UserInvitationDto> CreateInvitationAsync(CreateUserInvitationDto input);
        Task<CreateInvitationsResultDto> CreateInvitationsAsync(List<CreateUserInvitationDto> input);
        Task<ValidateInvitationResultDto> ValidateInvitationAsync(string token, int tenantId, string email);
    }
}
