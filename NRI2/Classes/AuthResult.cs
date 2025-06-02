using System.Data;

public class AuthResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
    public bool IsTwoFactorRequired { get; set; }
    public string SecretKey { get; set; }
    public DataTable User { get; set; }

    private AuthResult(bool success, string error, bool twoFactor, string secret)
    {
        IsSuccess = success;
        ErrorMessage = error;
        IsTwoFactorRequired = twoFactor;
        SecretKey = secret;
    }

    public static AuthResult Success(NRI.Classes.User user) => new(true, null, false, null);
    public static AuthResult TwoFactorRequired(string secret) => new(false, null, true, secret);
    public static AuthResult Failed(string error) => new(false, error, false, null);
}
