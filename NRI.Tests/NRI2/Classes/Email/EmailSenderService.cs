// EmailSenderService.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System;

namespace NRI.Classes.Email
{
    public class EmailSenderService : IEmailSenderService
    {
        private readonly IEmailTemplateService _templateService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailSenderService> _logger;

        public EmailSenderService(
            IEmailTemplateService templateService,
            IConfiguration configuration,
            ILogger<EmailSenderService> logger)
        {
            _templateService = templateService;
            _configuration = configuration;
            _logger = logger;
        }
        public async Task<bool> SendPasswordChangeConfirmationEmailAsync(string provider, string recipientEmail, string confirmationCode, string fullName = null)
        {
            string subject = "Подтверждение смены пароля";
            string body = _templateService.GetPasswordChangeConfirmationTemplate(confirmationCode, fullName);
            return await SendEmailAsync(provider, recipientEmail, subject, body, true);
        }

        public async Task<bool> SendTwoFactorSetupEmailAsync(string provider, string recipientEmail, string secretKey, string fullName = null)
        {
            string subject = "Настройка двухфакторной аутентификации";
            string body = _templateService.GetTwoFactorSetupTemplate(secretKey, fullName);
            return await SendEmailAsync(provider, recipientEmail, subject, body, true);
        }

        public async Task<bool> SendConfirmationEmailAsync(string provider, string recipientEmail, string confirmationCode, string fullName = null)
        {
            string subject = $"Подтверждение регистрации{(string.IsNullOrEmpty(fullName) ? "" : $" для {fullName}")}";
            string body = _templateService.GetRegistrationTemplate(confirmationCode, fullName);
            return await SendEmailAsync(provider, recipientEmail, subject, body, true);
        }

        public async Task<bool> SendPasswordResetEmailAsync(string provider, string recipientEmail, string resetLink)
        {
            string subject = "Сброс пароля";
            string body = _templateService.GetPasswordResetTemplate(resetLink);
            return await SendEmailAsync(provider, recipientEmail, subject, body, true);
        }

        public async Task<bool> SendEmailAsync(string provider, string recipientEmail, string subject, string body, bool isBodyHtml = true)
        {
            var settings = GetEmailSettings(provider);
            if (settings == null)
            {
                _logger.LogError("Email settings for provider {Provider} not found", provider);
                return false;
            }

            try
            {
                using var smtpClient = new SmtpClient(settings.SmtpHost)
                {
                    Port = settings.SmtpPort,
                    Credentials = new NetworkCredential(settings.SmtpUsername, settings.SmtpPassword),
                    EnableSsl = settings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 30000
                };

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11;

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(settings.FromEmail, settings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isBodyHtml
                };

                mailMessage.To.Add(recipientEmail);

                await smtpClient.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent via {Provider} to {Email}", provider, recipientEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email via {Provider} to {Email}", provider, recipientEmail);
                return false;
            }
        }

        public async Task<string> GetAvailableProviderAsync()
        {
            var providers = new[] { "Default", "Yandex" };
            foreach (var provider in providers)
            {
                if (await TestConnectionAsync(provider))
                    return provider;
            }
            return null;
        }

        public async Task<bool> TestConnectionAsync(string provider)
        {
            var settings = GetEmailSettings(provider);
            if (settings == null) return false;

            try
            {
                using var client = new SmtpClient(settings.SmtpHost)
                {
                    Port = settings.SmtpPort,
                    Credentials = new NetworkCredential(settings.SmtpUsername, settings.SmtpPassword),
                    EnableSsl = settings.EnableSsl,
                    Timeout = 5000
                };

                await client.SendMailAsync(
                    new MailMessage(settings.FromEmail, settings.FromEmail)
                    {
                        Subject = "Test connection",
                        Body = "This is a test email to verify SMTP connection"
                    });

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SMTP connection test failed for {Provider}", provider);
                return false;
            }
        }

        private EmailSettings GetEmailSettings(string provider)
        {
            try
            {
                return _configuration.GetSection($"EmailSettings:{provider}").Get<EmailSettings>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading email settings for provider {Provider}", provider);
                return null;
            }
        }
    }

    public class EmailSettings
    {
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; }
        public string SmtpPassword { get; set; }
        public bool EnableSsl { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
    }
}
