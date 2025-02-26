using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Net.Mail;
using Microsoft.Extensions.Logging;
using toyiyo.todo.Authorization.Users;

namespace toyiyo.todo.Notifications.Jobs
{
    public class EmailSendingJob : IBackgroundJob<EmailSendingArgs>, ITransientDependency
    {
        public void Execute(EmailSendingArgs args)
        {
            ExecuteAsync(args).Wait();
        }

        private readonly IEmailSender _sendGridEmailSender;
        private readonly UserManager _userManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ILogger _logger;

        public EmailSendingJob(
            IEmailSender sendGridEmailSender,
            UserManager userManager,
            IUnitOfWorkManager unitOfWorkManager,
            ILogger logger)
        {
            _sendGridEmailSender = sendGridEmailSender;
            _userManager = userManager;
            _unitOfWorkManager = unitOfWorkManager;
            _logger = logger;
        }

        public async Task ExecuteAsync(EmailSendingArgs args)
        {
            using (_unitOfWorkManager.Current.SetTenantId(args.TenantId))
            {
                var user = await _userManager.GetUserByIdAsync(args.UserId);
                if (user == null || string.IsNullOrEmpty(user.EmailAddress))
                {
                    _logger.LogWarning($"Could not send email - User {args.UserId} not found or has no email address");
                    return;
                }

                await _sendGridEmailSender.SendAsync(
                    to: user.EmailAddress,
                    subject: args.Subject,
                    body: args.Body,
                    isBodyHtml: true
                );
            }
        }
    }
}