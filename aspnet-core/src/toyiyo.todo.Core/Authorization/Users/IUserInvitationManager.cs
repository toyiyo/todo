using System.Threading.Tasks;
using toyiyo.todo.MultiTenancy;

namespace toyiyo.todo.Authorization.Users
{
    public interface IUserInvitationManager
    {
        public Task<UserInvitation> CreateInvitationAsync(Tenant tenant, string email, User invitedByUser);

    }
}