using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace NRI
{
    public partial class NotificationsWindow : Window
    {
        private readonly int _userId;
        private readonly INotificationService _notificationService;
        private readonly DataTable _notifications;

        public NotificationsWindow(DataTable notifications, IServiceProvider serviceProvider, int userId)
        {
            InitializeComponent();
            _userId = userId;
            _notifications = notifications;
            _notificationService = serviceProvider.GetRequiredService<INotificationService>();
            NotificationsGrid.ItemsSource = _notifications.DefaultView;
        }

        private async void MarkAsRead_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag != null && int.TryParse(button.Tag.ToString(), out var notificationId))
            {
                try
                {
                    await _notificationService.MarkAsReadAsync(notificationId);

                    // Обновляем статус в DataTable
                    foreach (DataRow row in _notifications.Rows)
                    {
                        if (row["NotificationID"] != DBNull.Value && (int)row["NotificationID"] == notificationId)
                        {
                            row["IsRead"] = true;
                            break;
                        }
                    }

                    NotificationsGrid.ItemsSource = _notifications.DefaultView;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
