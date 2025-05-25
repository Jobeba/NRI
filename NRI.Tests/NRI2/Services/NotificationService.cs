using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace NRI.Classes
{
    public class NotificationService : INotificationService
    {
        private readonly string _connectionString;

        public NotificationService(IConfigService configService)
        {
            _connectionString = configService.GetConnectionString();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(
                        "SELECT COUNT(*) FROM Notifications WHERE UserID = @UserID AND IsRead = 0",
                        connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userId);
                        return (int)await command.ExecuteScalarAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ошибка получения количества непрочитанных уведомлений", ex);
            }
        }

        public async Task<DataTable> GetUserNotificationsAsync(int userId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(
                        @"SELECT n.*, 
                        CASE 
                            WHEN n.NotificationType = 'Event' THEN 'Мероприятие'
                            WHEN n.NotificationType = 'System' THEN 'Системное'
                            ELSE n.NotificationType
                        END AS TypeDescription
                        FROM Notifications n
                        WHERE n.UserID = @UserID
                        ORDER BY n.CreatedDate DESC",
                        connection))
                    {
                        command.Parameters.AddWithValue("@UserID", userId);
                        var adapter = new SqlDataAdapter(command);
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        return dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ошибка получения уведомлений пользователя", ex);
            }
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand(
                        "UPDATE Notifications SET IsRead = 1 WHERE NotificationID = @NotificationID",
                        connection))
                    {
                        command.Parameters.AddWithValue("@NotificationID", notificationId);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Ошибка отметки уведомления как прочитанного", ex);
            }
        }
    }
}
