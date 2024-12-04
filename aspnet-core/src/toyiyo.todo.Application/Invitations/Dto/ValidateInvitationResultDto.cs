using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;

namespace toyiyo.todo.Invitations
{
    public class ValidateInvitationResultDto
    {
        [Required]
        public int TenantId { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
