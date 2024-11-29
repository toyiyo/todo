using System.Threading.Tasks;
using Abp.Authorization;
using toyiyo.todo.Authorization;
using toyiyo.todo.Authorization.Users;
using toyiyo.todo.Invitations.Dto;

namespace toyiyo.todo.Invitations
{
     [AbpAuthorize(PermissionNames.Pages_Subscription)]
    public class UserInvitationAppService : todoAppServiceBase, IUserInvitationAppService
    {
        private readonly IUserInvitationManager _userInvitationManager;

        public UserInvitationAppService(IUserInvitationManager userInvitationManager)
        {
            _userInvitationManager = userInvitationManager;
        }

        public async Task<UserInvitationDto> CreateInvitationAsync(CreateUserInvitationDto input)
        {
            var currentUser = await GetCurrentUserAsync();
            var tenant = await GetCurrentTenantAsync();
            var invitation = await _userInvitationManager.CreateInvitationAsync(tenant, input.Email, currentUser);
            return ObjectMapper.Map<UserInvitationDto>(invitation);
        }

    }
}
