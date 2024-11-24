using System.Threading.Tasks;

namespace toyiyo.todo.Authorization.Users
{
    internal interface IUserInvitationManager
    {
        public Task<UserInvitation> CreateInvitationAsync(int tenantId, string email, long invitedByUserId);

    }
}