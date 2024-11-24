
using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.Net.Mail;
using Abp.UI;
using toyiyo.todo.Core.Subscriptions;
using System.Linq.Dynamic.Core;
using Abp.Domain.Uow;

namespace toyiyo.todo.Authorization.Users
{
    public class UserInvitationManager : DomainService, IUserInvitationManager
    {
        private readonly IRepository<UserInvitation, Guid> _userInvitationRepository;
        private readonly UserManager _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ISubscriptionManager _subscriptionManager;

        public UserInvitationManager(
            IRepository<UserInvitation, Guid> userInvitationRepository,
            UserManager userManager,
            IEmailSender emailSender,
            ISubscriptionManager subscriptionManager)
        {
            _userInvitationRepository = userInvitationRepository;
            _userManager = userManager;
            _emailSender = emailSender;
            _subscriptionManager = subscriptionManager;
        }

        [UnitOfWork]
        public async Task<UserInvitation> CreateInvitationAsync(int tenantId, string email, long invitedByUserId)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    throw new UserFriendlyException("User with this email already exists");
                }

                // Check subscription seat limit
                var subscription = _subscriptionManager.GetSubscriptionByTenantId(tenantId);
                var totalSeats = subscription?.Items?.Data?.FirstOrDefault()?.Quantity ?? 0;
                var activeUserCount = _userManager.Users.Count(u => u.TenantId == tenantId && u.IsActive);
                var activeInvitesCount = await _userInvitationRepository.CountAsync(i => i.TenantId == tenantId && i.IsValid());

                if (activeUserCount + activeInvitesCount >= totalSeats)
                {
                    throw new UserFriendlyException("Subscription seat limit reached");
                }

                var invitation = UserInvitation.CreateDefaultInvitation(tenantId, email, await _userManager.GetUserByIdAsync(invitedByUserId));
                await _userInvitationRepository.InsertAsync(invitation);
                await SendInvitationEmailAsync(invitation);

                return invitation;
            }
            catch (System.Exception ex)
            {
                throw new UserFriendlyException("Error while creating invitation", ex.Message);
            }
        }

        private async Task SendInvitationEmailAsync(UserInvitation invitation)
        {
            var link = $"https://yourapp.com/Account/Register?token={invitation.Token}";
            var subject = "You are invited!";
            var body = $"Please click the following link to register: {link}";

            await _emailSender.SendAsync(invitation.Email, subject, body);
        }
    }
}