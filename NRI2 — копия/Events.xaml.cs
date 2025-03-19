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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Реализация сохранения
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            // Реализация удаления
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            NRI.Kasyanov_NRIDataSet kasyanov_NRIDataSet = ((NRI.Kasyanov_NRIDataSet)(this.FindResource("kasyanov_NRIDataSet")));
            // Загрузить данные в таблицу Events. Можно изменить этот код как требуется.
            NRI.Kasyanov_NRIDataSetTableAdapters.EventsTableAdapter kasyanov_NRIDataSetEventsTableAdapter = new NRI.Kasyanov_NRIDataSetTableAdapters.EventsTableAdapter();
            kasyanov_NRIDataSetEventsTableAdapter.Fill(kasyanov_NRIDataSet.Events);
            System.Windows.Data.CollectionViewSource eventsViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("eventsViewSource")));
            eventsViewSource.View.MoveCurrentToFirst();
        }
    }
}