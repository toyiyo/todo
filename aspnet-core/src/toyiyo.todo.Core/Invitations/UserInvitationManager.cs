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
using Abp.Timing;
using Microsoft.EntityFrameworkCore;
using toyiyo.todo.Authorization.Users;
using toyiyo.todo.MultiTenancy;

namespace toyiyo.todo.Invitations
{
    public class UserInvitationManager : DomainService, IUserInvitationManager
    {
        private readonly IRepository<UserInvitation, Guid> _userInvitationRepository;
        private readonly UserManager _userManager;

        public UserInvitationManager(
            IRepository<UserInvitation, Guid> userInvitationRepository,
            UserManager userManager)
        {
            _userInvitationRepository = userInvitationRepository;
            _userManager = userManager;
            LocalizationSourceName = todoConsts.LocalizationSourceName;
        }

        [UnitOfWork]
        public async Task<List<UserInvitation>> GetAll(GetAllUserInvitationsInput input)
        {
            return await GetAllUserInvitationsQueryable(input)
            .OrderBy<UserInvitation>(input?.Sorting ?? "CreationTime DESC")
            .Skip(input?.SkipCount ?? 0)
            .Take(input?.MaxResultCount ?? int.MaxValue)
            .ToListAsync();
        }

        [UnitOfWork]
        public async Task<int> GetAllCount(GetAllUserInvitationsInput input)
        {
            return await GetAllUserInvitationsQueryable(input).CountAsync();
        }

        private IQueryable<UserInvitation> GetAllUserInvitationsQueryable(GetAllUserInvitationsInput input)
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
            return await CreateUserInvitation(tenant, email, invitedByUser);
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

        private async Task<UserInvitation> GetUserInvitationByToken(string token)
        {
            return await _userInvitationRepository.FirstOrDefaultAsync(i => i.Token == token);
        }


        private async Task ValidateSubscriptionSeats(Tenant tenant, int newInvitationsCount = 1)
        {
            var activeUserCount = _userManager.Users.Count(u => u.IsActive);

            var activeInvitesCount = await _userInvitationRepository.CountAsync(i => i.Status == InvitationStatus.Pending && i.ExpirationDate > Clock.Now);

            if (activeUserCount + activeInvitesCount + newInvitationsCount >= tenant.SubscriptionSeats)
            {
                throw new InvalidOperationException(L("TenantMaximumUserCountReached{0}", tenant.SubscriptionSeats));
            }
        }

        private async Task<UserInvitation> HandleExistingInvitation(UserInvitation existingInvitation, User invitedByUser)
        {
            if (existingInvitation.Status == InvitationStatus.Pending && existingInvitation.ExpirationDate > Clock.Now)
            {
                throw new InvalidOperationException($"An invitation for this email is valid until {existingInvitation.ExpirationDate}");
            }

            if (existingInvitation.Status == InvitationStatus.Accepted)
            {
                throw new InvalidOperationException("This invitation has already been accepted");
            }

            if (existingInvitation.Status == InvitationStatus.Expired || existingInvitation.Status == InvitationStatus.Cancelled)
            {
                return await ReactivateInvitation(existingInvitation, invitedByUser);
            }

            throw new InvalidOperationException($"Invitation has an invalid status: {existingInvitation.Status}");
        }

        private async Task<UserInvitation> ReactivateInvitation(UserInvitation invitation, User reactivatedBy)
        {
            var reactivatedInvitation = UserInvitation.Reactivate(invitation, reactivatedBy);
            await _userInvitationRepository.UpdateAsync(reactivatedInvitation);
            return reactivatedInvitation;
        }

        private async Task<UserInvitation> CreateUserInvitation(Tenant tenant, string email, User invitedByUser)
        {
            var invitation = UserInvitation.CreateDefaultInvitation(tenant.Id, email, invitedByUser);
            await _userInvitationRepository.InsertAsync(invitation);

            return invitation;
        }

        public async Task<UserInvitation> FindByTokenAsync(string token)
        {
            return await _userInvitationRepository.FirstOrDefaultAsync(i => i.Token == token);
        }

        public async Task<UserInvitation> AcceptInvitation(string token, User acceptedBy)
        {
            var invitation = await GetUserInvitationByToken(token);
            invitation.Accept(acceptedBy);
            return await _userInvitationRepository.UpdateAsync(invitation);
        }

    }
}