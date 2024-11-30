
using System.Collections.Generic;

namespace toyiyo.todo.Invitations.Dto
{
    public class CreateInvitationsResultDto
    {
        public List<UserInvitationDto> Invitations { get; set; }
        public List<string> Errors { get; set; }

        public CreateInvitationsResultDto(List<UserInvitationDto> invitations, List<string> errors)
        {
            Invitations = invitations;
            Errors = errors;
        }
    }
}