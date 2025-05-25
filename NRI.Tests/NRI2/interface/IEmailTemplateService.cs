using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRI
{
    public interface IEmailTemplateService
    {
        string GetRegistrationTemplate(string confirmationCode, string fullName = null);
        string GetPasswordChangeConfirmationTemplate(string confirmationCode, string fullName = null);
        string GetTwoFactorSetupTemplate(string secretKey, string fullName = null);
        string GetPasswordResetTemplate(string resetLink);
    }
}


