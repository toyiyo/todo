
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
using toyiyo.todo.MultiTenancy;

namespace toyiyo.todo.Authorization.Users
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
        public async Task<UserInvitation> CreateInvitationAsync(Tenant tenant, string email, User invitedByUser)
        {
            try
            {
                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(email);
                if (existingUser != null)
                {
                    throw new UserFriendlyException("User with this email already exists");
                }

                var totalSeats = tenant.SubscriptionSeats;
                var activeUserCount = _userManager.Users.Count(u => u.IsActive);
                var activeInvitesCount = await _userInvitationRepository.CountAsync(i => i.IsValid());

                if (activeUserCount + activeInvitesCount >= totalSeats)
                {
                    throw new UserFriendlyException("Subscription seat limit reached");
                }

                var invitation = UserInvitation.CreateDefaultInvitation(tenant.Id, email, invitedByUser);
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