using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.UI;
using toyiyo.todo.Authorization;
using toyiyo.todo.Authorization.Users;

namespace toyiyo.todo.Users
{
     [AbpAuthorize(PermissionNames.Pages_Subscription)]
    public class UserInvitationAppService : todoAppServiceBase, IUserInvitationService
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
