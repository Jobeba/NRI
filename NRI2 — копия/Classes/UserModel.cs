// UserModel.cs
namespace NRI.Classes
{
    public class UserModel
    {
        public string Login { get; set; }
        public string Email { get; set; }
        public bool IsPasswordConfirmed { get; set; }
        public string TwoFactorSecret { get; set; }
    }
}
