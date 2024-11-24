using System.Threading.Tasks;

namespace toyiyo.todo.Users
{
    internal interface IUserInvitationService
    {
        public Task<UserInvitationDto> CreateInvitationAsync(CreateUserInvitationDto input)
    }
}
