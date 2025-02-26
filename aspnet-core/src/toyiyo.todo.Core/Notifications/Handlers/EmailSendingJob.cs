using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Net.Mail;

namespace toyiyo.todo.Notifications.Jobs
{
    public class EmailSendingJob : BackgroundJob<EmailSendingArgs>, ITransientDependency
    {
        private readonly IEmailSender _sendGridEmailSender;

        public override void Execute(EmailSendingArgs args)
        {
            Task.Run(() => ExecuteEmailSendingAsync(args).GetAwaiter().GetResult()).Wait();
        }
        public EmailSendingJob(IEmailSender sendGridEmailSender)
        {
            _sendGridEmailSender = sendGridEmailSender;
        }

        public async Task ExecuteEmailSendingAsync(EmailSendingArgs args)
        {
            await _sendGridEmailSender.SendAsync(
                args.EmailAddress,
                args.Subject,
                args.Body,
                true
            );
        }
    }
}