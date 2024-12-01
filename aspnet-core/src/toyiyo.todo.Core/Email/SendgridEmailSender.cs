using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Abp.Dependency;
using Abp.Net.Mail;
using Abp.Net.Mail.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace toyiyo.todo.Email
{
    public class SendGridEmailSender : EmailSenderBase, ITransientDependency
    {
        private readonly string _sendGridApiKey;
        private readonly string _fromEmail;
        private readonly string _senderDisplayName;
        private readonly ILogger<SendGridEmailSender> _logger;

        public SendGridEmailSender(
            IEmailSenderConfiguration configuration,
            IConfiguration appConfiguration,
            ILogger<SendGridEmailSender> logger)
            : base(configuration)
        {
            _sendGridApiKey = appConfiguration["SendgridApiKey"];
            _fromEmail = appConfiguration["FromTransactionalEmail"];
            _senderDisplayName = appConfiguration["SenderDisplayName"];
            _logger = logger;

            if (string.IsNullOrEmpty(_sendGridApiKey))
            {
                throw new InvalidOperationException("SendGrid API Key is not configured.");
            }
        }

        protected override void SendEmail(MailMessage mailMessage)
        {
            SendEmailAsync(mailMessage).GetAwaiter().GetResult();
        }

        protected override async Task SendEmailAsync(MailMessage mailMessage)
        {
            try
            {
                var client = new SendGridClient(_sendGridApiKey);
                var from = new EmailAddress(_fromEmail, _senderDisplayName);
                var to = new EmailAddress(mailMessage.To[0].Address, mailMessage.To[0].DisplayName); // Single recipient example
                var subject = mailMessage.Subject;
                var plainTextContent = mailMessage.Body;
                var htmlContent = mailMessage.IsBodyHtml ? mailMessage.Body : null;

                var sendGridMessage = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

                var response = await client.SendEmailAsync(sendGridMessage);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Email sending failed. Status Code: {response.StatusCode}");
                    throw new InvalidOperationException($"Email sending failed. Status Code: {response.StatusCode}");
                }

                _logger.LogInformation($"Email sent successfully to {to.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending email.");
                throw new InvalidOperationException("An error occurred while sending email.", ex);
            }
        }
    }
}