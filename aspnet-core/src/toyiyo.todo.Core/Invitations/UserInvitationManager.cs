using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Abp.Net.Mail;
using Abp.Timing;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using toyiyo.todo.Authorization.Users;
using toyiyo.todo.MultiTenancy;

namespace toyiyo.todo.Invitations
{
    public class UserInvitationManager : DomainService, IUserInvitationManager
    {
        private readonly IRepository<UserInvitation, Guid> _userInvitationRepository;
        private readonly UserManager _userManager;
        private readonly IEmailSender _emailSender;

        public UserInvitationManager(
            IRepository<UserInvitation, Guid> userInvitationRepository,
            UserManager userManager,
            IEmailSender emailSender)
        {
            _userInvitationRepository = userInvitationRepository;
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [UnitOfWork]
        public async Task<List<UserInvitation>> GetAll(GetAllUserInvitationsInput input)
        {
            return await GetAllJobsQueryable(input)
            .OrderBy<UserInvitation>(input?.Sorting ?? "CreationTime DESC")
            .Skip(input?.SkipCount ?? 0)
            .Take(input?.MaxResultCount ?? int.MaxValue)
            .ToListAsync();
        }

        [UnitOfWork]
        public async Task<int> GetAllCount(GetAllUserInvitationsInput input)
        {
            return await GetAllJobsQueryable(input).CountAsync();
        }

        private IQueryable<UserInvitation> GetAllJobsQueryable(GetAllUserInvitationsInput input)
        {
            //repository methods already filter by tenant, we can check other attributes by adding "or" "||" to the whereif clause
            var query = _userInvitationRepository.GetAll()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), p => p.Email.ToUpper().Contains(input.Keyword.ToUpper()));

            return query;
        }

        private async Task<UserInvitation> ProcessSingleInvitation(Tenant tenant, string email, User invitedByUser)
        {
            await ValidateInvitationRequest(tenant, email, invitedByUser);
            
            var existingInvitation = await FindExistingInvitation(email);
            if (existingInvitation != null)
            {
                return await HandleExistingInvitation(existingInvitation, invitedByUser);
            }
            
            return UserInvitation.CreateDefaultInvitation(tenant.Id, email, invitedByUser);
        }

        [UnitOfWork]
        public async Task<(List<UserInvitation> Invitations, List<string> Errors)> CreateInvitationsAsync(Tenant tenant, List<string> emails, User invitedByUser)
        {
            var invitations = new List<UserInvitation>();
            var errors = new List<string>();

            try
            {
                await ValidateSubscriptionSeats(tenant, emails.Count);
            }
            catch (Exception ex)
            {
                errors.Add($"Error validating subscription seats: {ex.Message}");
                return (invitations, errors);
            }

            foreach (var email in emails.Distinct())
            {
                try
                {
                    var invitation = await ProcessSingleInvitation(tenant, email, invitedByUser);
                    invitations.Add(invitation);
                }
                catch (Exception ex)
                {
                    errors.Add($"Error processing invitation for {email}: {ex.Message}");
                }
            }

            var newInvitations = invitations.Where(i => i.Id == default).ToList();
            if (newInvitations.Any())
            {
                foreach (var newInvitation in newInvitations)
                {
                    await _userInvitationRepository.InsertAsync(newInvitation);
                }
            }

            await Task.WhenAll(invitations.Select(invitation => SendInvitationEmailAsync(invitation)));

            return (invitations, errors);
        }

        [UnitOfWork]
        public async Task<UserInvitation> CreateInvitationAsync(Tenant tenant, string email, User invitedByUser)
        {
            await ValidateInvitationRequest(tenant, email, invitedByUser);

            var existingInvitation = await FindExistingInvitation(email);
            if (existingInvitation != null)
            {
                return await HandleExistingInvitation(existingInvitation, invitedByUser);
            }

            await ValidateSubscriptionSeats(tenant);
            return await CreateAndSendNewInvitation(tenant, email, invitedByUser);
        }
        private async Task ValidateInvitationRequest(Tenant tenant, string email, User invitedByUser)
        {
            if (tenant == null) throw new ArgumentNullException(nameof(tenant));
            if (string.IsNullOrWhiteSpace(email)) throw new ArgumentNullException(nameof(email));
            if (invitedByUser == null) throw new ArgumentNullException(nameof(invitedByUser));

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with email {email} is already registered");
            }
        }

        private async Task<UserInvitation> FindExistingInvitation(string email)
        {
            return await _userInvitationRepository.FirstOrDefaultAsync(i => i.Email == email);
        }

        private async Task ValidateSubscriptionSeats(Tenant tenant, int newInvitationsCount = 1)
        {
            var activeUserCount = _userManager.Users.Count(u => u.IsActive);

            var activeInvitesCount = await _userInvitationRepository.CountAsync(i => i.Status == InvitationStatus.Pending && i.ExpirationDate > Clock.Now);

            if (activeUserCount + activeInvitesCount + newInvitationsCount >= tenant.SubscriptionSeats)
            {
                throw new InvalidOperationException($"Subscription limit reached: Your subscription limit of {tenant.SubscriptionSeats} users has been reached");
            }
        }

        private async Task<UserInvitation> HandleExistingInvitation(UserInvitation existingInvitation, User invitedByUser)
        {
            return existingInvitation.Status switch
            {
                InvitationStatus.Pending when existingInvitation.ExpirationDate > Clock.Now => throw new InvalidOperationException($"An invitation for this email is valid until {existingInvitation.ExpirationDate}"),
                InvitationStatus.Accepted => throw new InvalidOperationException("This invitation has already been accepted"),
                InvitationStatus.Expired or InvitationStatus.Cancelled => await ReactivateInvitation(existingInvitation, invitedByUser),
                _ => throw new InvalidOperationException($"Invitation has an invalid status: {existingInvitation.Status}"),
            };
        }

        private async Task<UserInvitation> ReactivateInvitation(UserInvitation invitation, User reactivatedBy)
        {
            var reactivatedInvitation = UserInvitation.Reactivate(invitation, reactivatedBy);
            await _userInvitationRepository.UpdateAsync(reactivatedInvitation);
            await SendInvitationEmailAsync(reactivatedInvitation);
            return reactivatedInvitation;
        }

        private async Task<UserInvitation> CreateAndSendNewInvitation(Tenant tenant, string email, User invitedByUser)
        {
            var invitation = UserInvitation.CreateDefaultInvitation(tenant.Id, email, invitedByUser);
            await _userInvitationRepository.InsertAsync(invitation);
            await SendInvitationEmailAsync(invitation);

            return invitation;
        }

        private async Task SendInvitationEmailAsync(UserInvitation invitation)
        {
            var subject = "You've been invited to join Toyiyo, your simple project management app";
            var registerUrl = $"https://toyiyo.io/account/register?token={invitation.Token}";
            
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
                    <p style='color: #666; font-size: 12px;'>This invitation will expire in {invitation.ExpirationDate:dd} days.</p>
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
    }
}