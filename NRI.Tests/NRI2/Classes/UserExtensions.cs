using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRI.Classes
{
    public static class UserExtensions
    {
        public static string GetLogin(this User user)
        {
            return user.Login;
        }
    }
}
