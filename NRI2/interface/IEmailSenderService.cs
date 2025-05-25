using System.Threading.Tasks;

namespace NRI.Classes.Email
{
    public interface IEmailSenderService
    {
        Task<bool> SendTwoFactorSetupEmailAsync(string provider, string recipientEmail, string secretKey, string fullName = null);
        Task<bool> SendEmailAsync(string provider, string recipientEmail, string subject, string body, bool isBodyHtml = true);
        Task<bool> SendConfirmationEmailAsync(string provider, string recipientEmail, string confirmationCode, string fullName = null);
        Task<bool> SendPasswordResetEmailAsync(string provider, string recipientEmail, string resetLink);
        Task<string> GetAvailableProviderAsync();
        Task<bool> TestConnectionAsync(string provider);
    }
}
