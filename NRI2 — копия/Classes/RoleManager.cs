using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRI.Classes
{
    public static class RoleManager
    {
        // Иерархия ролей от высшей к низшей
        private static readonly Dictionary<string, int> RoleHierarchy = new Dictionary<string, int>
    {
        { "Администратор", 3 },
        { "Организатор", 2 },
        { "Игрок", 1 }
    };

        public static string GetHighestRole(List<string> roles)
        {
            if (roles == null || !roles.Any())
                return "Игрок"; // Роль по умолчанию

            return roles
                .OrderByDescending(r => RoleHierarchy.ContainsKey(r) ? RoleHierarchy[r] : 0)
                .FirstOrDefault() ?? "Игрок";
        }
    }
}
