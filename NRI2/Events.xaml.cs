using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using NRI.ViewModels;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using NRI.Models;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Controls;

namespace NRI
{
    public partial class Events : Window
    {
        private readonly EventsViewModel _viewModel;
        private readonly ILogger<Models.Events> _logger;
        private readonly EventContext _context;

        public Events(EventsViewModel viewModel, ILogger<Models.Events> logger, EventContext context)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _logger = logger;
            _context = context;
            DataContext = _viewModel;
            LoadEvents();
        }

        private async void LoadEvents()
        {
            try
            {
                var events = await _context.Events.ToListAsync();
                _viewModel.Events = new ObservableCollection<Models.Events>(events);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Сохраняем изменения в базе данных
                    await _context.SaveChangesAsync();

                    // Фиксируем транзакцию
                    await transaction.CommitAsync();

                    MessageBox.Show("Изменения успешно сохранены.");
                }
                catch (Exception ex)
                {
                    // Откатываем транзакцию в случае ошибки
                    await transaction.RollbackAsync();
                    MessageBox.Show($"Ошибка при сохранении: {ex.Message}");
                }
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedEvent = eventsDataGrid.SelectedItem as Models.Events;

                if (selectedEvent == null)
                {
                    MessageBox.Show("Выберите событие для удаления.");
                    return;
                }

                _context.Events.Remove(selectedEvent);
                await _context.SaveChangesAsync();
                _viewModel.Events.Remove(selectedEvent);

                MessageBox.Show("Событие успешно удалено.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}");
            }
        }

        private void MainMenu_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = App.ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            Close();
        }

        private void Showdown_click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _logger.LogInformation("Окно событий закрыто.");
        }
    }
}