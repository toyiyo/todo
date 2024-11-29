using System.Threading.Tasks;
using toyiyo.todo.Authorization.Users;
using toyiyo.todo.MultiTenancy;

namespace toyiyo.todo.Invitations
{
    public interface IUserInvitationManager
    {
        public Task<UserInvitation> CreateInvitationAsync(Tenant tenant, string email, User invitedByUser);

    }
}