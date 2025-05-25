using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class LoginResult
{
    public bool IsSuccess { get; set; }
    public bool IsTwoFactorRequired { get; set; }
    public string SecretKey { get; set; }
    public string ErrorMessage { get; set; }
    public DataTable User { get; set; }

    public static LoginResult Success(DataTable user) => new() { IsSuccess = true, User = user };
    public static LoginResult TwoFactorRequired(string secretKey) => new() { IsTwoFactorRequired = true, SecretKey = secretKey };
    public static LoginResult Failed(string error) => new() { ErrorMessage = error };
}
