using System;
using System.ComponentModel.DataAnnotations;
using Abp.AutoMapper;
using toyiyo.todo.Authorization.Users;

namespace toyiyo.todo.Users
{
    /// <summary>
    /// Data transfer object for user invitation requests.
    /// </summary>
    /// <remarks>
    /// This DTO is used to transfer user invitation data between layers of the application.
    /// It contains the essential information needed to invite a new user.
    /// </remarks>
    [AutoMapFrom(typeof(UserInvitation))]
    public class UserInvitationDto
    {
        public virtual int TenantId { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; protected set; }
        // Keep as ID to avoid navigation property complexity
        [Required]
        public long InvitedByUserId { get; protected set; }

        [Required]
        public DateTime ExpirationDate { get; protected set; }
        public DateTime? AcceptedDate { get; protected set; }
        [Required]
        public string Token { get; protected set; }
        [Required]
        public InvitationStatus Status { get; protected set; }
    }
}
