using Abp.Application.Services.Dto;

namespace toyiyo.todo.Invitations
{
    public class GetAllUserInvitationsInput : PagedAndSortedResultRequestDto
    {
        public string Keyword { get; set; } 
    }
}