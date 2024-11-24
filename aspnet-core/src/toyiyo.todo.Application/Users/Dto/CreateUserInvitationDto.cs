using System.ComponentModel.DataAnnotations;

namespace toyiyo.todo.Users
{
    /// <summary>
    /// Data transfer object for creating a user invitation.
    /// </summary>
    /// <remarks>
    /// This DTO contains the required email address for sending an invitation to a new user.
    /// </remarks>
    public class CreateUserInvitationDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
