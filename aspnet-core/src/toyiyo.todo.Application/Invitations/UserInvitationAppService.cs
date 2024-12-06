using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Extensions;
using Abp.Net.Mail;
using Abp.UI;
using Microsoft.Extensions.Configuration;
using toyiyo.todo.Authorization;
using toyiyo.todo.Invitations.Dto;
using toyiyo.todo.MultiTenancy;

namespace toyiyo.todo.Invitations
{
    [AbpAuthorize(PermissionNames.Pages_Subscription)]
    public class UserInvitationAppService : todoAppServiceBase, IUserInvitationAppService
    {
        private readonly IUserInvitationManager _userInvitationManager;
        private readonly IEmailSender _emailSender;
        private readonly string _baseUrl;

        public UserInvitationAppService(IUserInvitationManager userInvitationManager, IEmailSender emailSender, IConfiguration configuration)
        {
            _userInvitationManager = userInvitationManager;
            _emailSender = emailSender;
            _baseUrl = configuration["ClientRootAddress"] ?? "https://app.toyiyo.com";
        }

        public async Task<PagedResultDto<UserInvitationDto>> GetAll(GetAllUserInvitationsInput input)
        {
            var invitations = await _userInvitationManager.GetAll(input);
            var invitationsTotalCount = await _userInvitationManager.GetAllCount(input);
            return new PagedResultDto<UserInvitationDto>(invitationsTotalCount, ObjectMapper.Map<List<UserInvitationDto>>(invitations));
        }

        public async Task<UserInvitationDto> CreateInvitationAsync(CreateUserInvitationDto input)
        {
            var currentUser = await GetCurrentUserAsync();
            var tenant = await GetCurrentTenantAsync();
            var invitation = await _userInvitationManager.CreateInvitationAsync(tenant, input.Email, currentUser);
            return ObjectMapper.Map<UserInvitationDto>(invitation);
        }

        public async Task<CreateInvitationsResultDto> CreateInvitationsAsync(List<CreateUserInvitationDto> input)
        {
            var currentUser = await GetCurrentUserAsync();
            var tenant = await GetCurrentTenantAsync();
            var emails = input.Select(x => x.Email).ToList();
            var (invitations, errors) = await _userInvitationManager.CreateInvitationsAsync(tenant, emails, currentUser);

            await Task.WhenAll(invitations.Select(async invitation =>
            {
                try
                {
                    await SendInvitationEmailAsync(invitation, tenant);
                }
                catch (Exception ex)
                {
                    errors.Add($"Failed to send invitation email to {invitation.Email}: {ex.Message}");
                }
            }));

            return new CreateInvitationsResultDto(
                ObjectMapper.Map<List<UserInvitationDto>>(invitations),
                errors
            );
        }
        private async Task SendInvitationEmailAsync(UserInvitation invitation, Tenant tenant)
        {
            var subject = "You've been invited to join Toyiyo, your simple project management app";
            var registerUrl = $"{_baseUrl}/account/register?token={Uri.EscapeDataString(invitation.Token)}&tenant={Uri.EscapeDataString(tenant.Name)}";

            var htmlBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2>You've been invited!</h2>
                    <p>You've been invited to join Toyiyo. Click the button below to get started:</p>
                    <div style='margin: 25px 0;'>
                        <a href='{registerUrl}' style='background-color: #4CAF50; color: white; padding: 12px 25px; text-decoration: none; border-radius: 4px;'>
                            Accept Invitation
                        </a>
                    </div>
                    <p>Or copy and paste this URL into your browser:</p>
                    <p style='color: #666; word-break: break-all;'>{registerUrl}</p>
                    <p style='color: #666; font-size: 12px;'>This invitation will expire on {invitation.ExpirationDate:D}.</p>
                </div>";

            var message = new System.Net.Mail.MailMessage
            {
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(invitation.Email);
            await _emailSender.SendAsync(message);
        }
        [AbpAllowAnonymous]
        public async Task<ValidateInvitationResultDto> ValidateInvitationAsync(string token, string email = "")
        {
            var invitation = await _userInvitationManager.FindByTokenAsync(token);
            if (invitation == null || !invitation.ValidateToken(token))
            {
                throw new UserFriendlyException("Invalid or expired invitation token.");
            }
            if (!email.IsNullOrEmpty() && !invitation.Email.Equals(email, StringComparison.OrdinalIgnoreCase))
            {
                throw new UserFriendlyException("Invalid invitation token for this email.");
            }

            return new ValidateInvitationResultDto
            {
                TenantId = invitation.TenantId,
                Email = invitation.Email
            };
        }

    }
}
