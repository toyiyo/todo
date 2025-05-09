using System.Collections.Generic;
using System.Threading.Tasks;
using toyiyo.todo.Authorization.Users;
using toyiyo.todo.MultiTenancy;

namespace toyiyo.todo.Invitations
{
    public interface IUserInvitationManager
    {
        public Task<UserInvitation> AcceptInvitation(string token, User acceptedBy);
        public Task<UserInvitation> CreateInvitationAsync(Tenant tenant, string email, User invitedByUser);
        public  Task<(List<UserInvitation> Invitations, List<string> Errors)> CreateInvitationsAsync(Tenant tenant, List<string> emails, User invitedByUser);
        Task<UserInvitation>FindByTokenAsync(string token);
        public Task<List<UserInvitation>> GetAll(GetAllUserInvitationsInput input);
        public Task<int> GetAllCount(GetAllUserInvitationsInput input);
    }
}