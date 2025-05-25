using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NRI.Controls;

namespace NRI.Pages
{
    public partial class ProjectsPage : Page
    {
        private int _unreadCount;
        private bool _isPlayer;
        private int _currentUserId;
        private readonly ILogger<ProjectsPage> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _connectionString;
        private readonly ObservableCollection<ToastNotification> _activeToasts = new();

        public class ToastNotification
        {
            public string Title { get; set; }
            public string Message { get; set; }
        }

        public ProjectsPage(IServiceProvider serviceProvider, ILogger<ProjectsPage> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionString = _serviceProvider.GetRequiredService<IConfigService>().GetConnectionString();

            InitializeComponent();
            ToastContainer.ItemsSource = _activeToasts;
            LoadEventsData();
            _ = CheckForNewNotificationsAsync(); // Запускаем проверку уведомлений

            CheckUserRole();

            if (_isPlayer)
            {
                _ = LoadUnreadNotificationsCount();
            }
            else
            {
                NotificationsButton.Visibility = Visibility.Collapsed;
            }

            InitializeUserRoleAndNotifications();

            _logger.LogInformation("ProjectsPage инициализирован");
        }
        private void InitializeUserRoleAndNotifications()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();

                    // Проверяем, является ли пользователь игроком
                    var roleCommand = new SqlCommand(
                        @"SELECT COUNT(*) FROM UserRoles ur
                    JOIN Roles r ON ur.RoleID = r.RoleID
                    WHERE ur.UserID = @UserID AND r.RoleName = 'Игрок'",
                        connection);
                    roleCommand.Parameters.AddWithValue("@UserID", _currentUserId);
                    _isPlayer = (int)roleCommand.ExecuteScalar() > 0;

                    if (_isPlayer)
                    {
                        // Загружаем количество непрочитанных уведомлений
                        var countCommand = new SqlCommand(
                            "SELECT COUNT(*) FROM Notifications WHERE UserID = @UserID AND IsRead = 0",
                            connection);
                        countCommand.Parameters.AddWithValue("@UserID", _currentUserId);
                        _unreadCount = (int)countCommand.ExecuteScalar();

                        UpdateNotificationBadge();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка инициализации роли и уведомлений");
                _isPlayer = false;
                _unreadCount = 0;
            }

            NotificationsButton.Visibility = _isPlayer ? Visibility.Visible : Visibility.Collapsed;
        }
        private void UpdateNotificationBadge()
        {
            Dispatcher.Invoke(() =>
            {
                if (_unreadCount > 0)
                {
                    NotificationBadge.Visibility = Visibility.Visible;
                    BadgeCount.Text = _unreadCount.ToString();
                }
                else
                {
                    NotificationBadge.Visibility = Visibility.Collapsed;
                }
            });
        }

        private void CheckUserRole()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(
                        @"SELECT COUNT(*) FROM UserRoles ur
                    JOIN Roles r ON ur.RoleID = r.RoleID
                    WHERE ur.UserID = @UserID AND r.RoleName = 'Игрок'",
                        connection);
                    command.Parameters.AddWithValue("@UserID", _currentUserId);

                    _isPlayer = (int)command.ExecuteScalar() > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка проверки роли пользователя");
                _isPlayer = false;
            }
        }

        private async Task LoadUnreadNotificationsCount()
        {
            try
            {
                var notificationService = _serviceProvider.GetRequiredService<INotificationService>();
               _unreadCount = await notificationService.GetUnreadCountAsync(_currentUserId);

                Dispatcher.Invoke(() =>
                {
                    if (_unreadCount > 0)
                    {
                        NotificationBadge.Visibility = Visibility.Visible;
                        BadgeCount.Text = _unreadCount.ToString();
                    }
                    else
                    {
                        NotificationBadge.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки количества уведомлений");
            }
        }

        public void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.LoadContentBasedOnRole();
                mainWindow.ToggleMenuCommand.Execute(null);
            }
        }

        private async void ShowNotifications_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var notificationService = _serviceProvider.GetRequiredService<INotificationService>();
                var notifications = await notificationService.GetUserNotificationsAsync(_currentUserId);

                var window = new NotificationsWindow(notifications, _serviceProvider, _currentUserId)
                {
                    Owner = Window.GetWindow(this)
                };

                if (window.ShowDialog() == true)
                {
                    // Обновляем счетчик после закрытия окна
                    _unreadCount = await notificationService.GetUnreadCountAsync(_currentUserId);
                    UpdateNotificationBadge();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки уведомлений");
                ShowToast("Ошибка", "Не удалось загрузить уведомления");
            }
        }

        private void ShowToast(string title, string message)
        {
            Dispatcher.Invoke(() =>
            {
                var toast = new ToastNotification
                {
                    Title = title,
                    Message = message
                };

                _activeToasts.Add(toast);

                var timer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(5)
                };

                timer.Tick += (sender, args) =>
                {
                    _activeToasts.Remove(toast);
                    timer.Stop();
                };

                timer.Start();
            });
        }

        private async Task CheckForNewNotificationsAsync()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    var command = new SqlCommand(
                        "SELECT COUNT(*) FROM Notifications WHERE UserID = @UserID AND IsRead = 0",
                        connection);
                    command.Parameters.AddWithValue("@UserID", _currentUserId);

                    var unreadCount = (int)await command.ExecuteScalarAsync();

                    if (unreadCount > 0)
                    {
                        ShowToast("Новые уведомления",
                            $"У вас {unreadCount} непрочитанных уведомлений");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка проверки уведомлений");
            }
        }
        private bool CheckUserRole(string requiredRole)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand(
                        @"SELECT COUNT(*) FROM UserRoles ur
                            JOIN Roles r ON ur.RoleID = r.RoleID
                            WHERE ur.UserID = @UserID AND r.RoleName = @RoleName",
                        connection);
                    command.Parameters.AddWithValue("@UserID", _currentUserId);
                    command.Parameters.AddWithValue("@RoleName", requiredRole);

                    return (int)command.ExecuteScalar() > 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка проверки роли пользователя");
                return false;
            }
        }


        private async void RegisterForEvent_Click(object sender, RoutedEventArgs e)
        {
            if (eventsDataGrid.SelectedItem == null)
            {
                ShowToast("Ошибка", "Выберите мероприятие для записи");
                return;
            }

            try
            {
                var rowView = (DataRowView)eventsDataGrid.SelectedItem;
                int eventId = (int)rowView["EventID"];
                string eventName = rowView["EventName"].ToString();

                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();

                    // Проверка наличия свободных мест
                    var checkCommand = new SqlCommand(
                        @"SELECT COUNT(*) FROM EventParticipants 
                        WHERE EventID = @EventID AND Status = 'Registered'",
                        connection);
                    checkCommand.Parameters.AddWithValue("@EventID", eventId);

                    int registeredCount = (int)await checkCommand.ExecuteScalarAsync();

                    int maxParticipants = (int)rowView["MaxParticipants"];

                    if (registeredCount >= maxParticipants)
                    {
                        ShowToast("Ошибка", "На мероприятие нет свободных мест");
                        return;
                    }

                    // Запись пользователя
                    var insertCommand = new SqlCommand(
                        "INSERT INTO EventParticipants (EventID, UserID) VALUES (@EventID, @UserID)",
                        connection);
                    insertCommand.Parameters.AddWithValue("@EventID", eventId);
                    insertCommand.Parameters.AddWithValue("@UserID", _currentUserId);

                    await insertCommand.ExecuteNonQueryAsync();

                    // Создаем уведомление в базе
                    var notificationCommand = new SqlCommand(
                        @"INSERT INTO Notifications (UserID, Title, Message, NotificationType)
                        VALUES (@UserID, 'Запись на мероприятие', 
                        @Message, 'Event')",
                        connection);
                    notificationCommand.Parameters.AddWithValue("@UserID", _currentUserId);
                    notificationCommand.Parameters.AddWithValue("@Message",
                        $"Вы записаны на мероприятие '{eventName}'");

                    await notificationCommand.ExecuteNonQueryAsync();

                    ShowToast("Успешно", $"Вы записаны на '{eventName}'");
                }
            }
            catch (Exception ex)
            {
                ShowToast("Ошибка", $"Не удалось записаться: {ex.Message}");
                _logger.LogError(ex, "Ошибка записи на мероприятие");
            }
        }

        // Остальные методы остаются без изменений
        private void LoadEventsData()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    var command = new SqlCommand("SELECT * FROM Events", connection);
                    var adapter = new SqlDataAdapter(command);
                    var dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    eventsDataGrid.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                ShowToast("Ошибка", $"Ошибка загрузки данных: {ex.Message}");
                _logger.LogError(ex, "Ошибка загрузки данных мероприятий");
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Реализация сохранения изменений в БД
                MessageBox.Show("Мероприятие сохранено", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (eventsDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите мероприятие для удаления", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить это мероприятие?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    var rowView = (DataRowView)eventsDataGrid.SelectedItem;
                    int eventId = (int)rowView["EventID"];

                    using (var connection = new SqlConnection(_connectionString))
                    {
                        connection.Open();
                        var command = new SqlCommand("DELETE FROM Events WHERE EventID = @EventID", connection);
                        command.Parameters.AddWithValue("@EventID", eventId);
                        command.ExecuteNonQuery();
                    }

                    LoadEventsData(); // Обновляем данные после удаления
                    MessageBox.Show("Мероприятие удалено", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void MainMenu_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.MainFrame.Content = null;
        }

        private void Showdown_click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

    }
}
