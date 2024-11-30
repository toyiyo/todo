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

        [UnitOfWork]
        public async Task<(List<UserInvitation> Invitations, List<string> Errors)> CreateInvitationsAsync(Tenant tenant, List<string> emails, User invitedByUser)
        {
            var invitations = new List<UserInvitation>();
            var errors = new List<string>();

            // Check subscription seats once
            try
            {
                await ValidateSubscriptionSeats(tenant, emails.Count);
            }
            catch (Exception ex)
            {
                errors.Add($"Error validating subscription seats: {ex.Message}");
                return (invitations, errors); // Return early if subscription validation fails
            }

            // Validate each email
            foreach (var email in emails)
            {
                try
                {
                    await ValidateInvitationRequest(tenant, email, invitedByUser);
                }
                catch (Exception ex)
                {
                    errors.Add($"Error validating email {email}: {ex.Message}");
                    continue; // Skip to next email if validation fails
                }

                try
                {
                    var existingInvitation = await FindExistingInvitation(email);
                    if (existingInvitation != null)
                    {
                        var handledInvitation = await HandleExistingInvitation(existingInvitation, invitedByUser);
                        invitations.Add(handledInvitation);
                    }
                    else
                    {
                        var newInvitation = UserInvitation.CreateDefaultInvitation(tenant.Id, email, invitedByUser);
                        invitations.Add(newInvitation);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Error handling invitation for email {email}: {ex.Message}");
                }
            }

            // Insert all new invitations
            foreach (var invitation in invitations.Where(i => i.Id == default))
            {
                await _userInvitationRepository.InsertAsync(invitation);
            }
            // Send emails in parallel
            var emailTasks = invitations.Select(invitation => SendInvitationEmailAsync(invitation));
            await Task.WhenAll(emailTasks);

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
            //todo: setup email sending
            var link = $"https://yourapp.com/Account/Register?token={invitation.Token}";
            var subject = "You are invited!";
            var body = $"Please click the following link to register: {link}";
            //todo: setup email sending
            return;//_emailSender.SendAsync(invitation.Email, subject, body);
        }
    }
}