using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRI
{
    interface UserModel
    {
        Task<UserModel> GetUserModelAsync(string login);
    }
}
