using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRI
{
    public interface INotificationService
    {
        Task<int> GetUnreadCountAsync(int userId);
        Task<DataTable> GetUserNotificationsAsync(int userId);
        Task MarkAsReadAsync(int notificationId);
    }
}
