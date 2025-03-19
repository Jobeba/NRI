using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NRI.ViewModel;
using System;
using System.Windows;
using WpfAnimatedGif;

namespace NRI
{
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MainWindow> _logger;
        private readonly INavigationService _navigationService;
        private readonly INavigationService navigationService;

        public MainWindow(IServiceProvider serviceProvider, ILogger<MainWindow> logger, INavigationService navigationService)
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(serviceProvider);
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

            _logger.LogInformation("Главное меню запустилось");
        }

        private void Staff_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogInformation("Открытие окна сотрудников");
            _navigationService.NavigateTo<staff>();
        }

        private void Reviews_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogInformation("Открытие окна отзывов");
            _navigationService.NavigateTo<Reviews>();
        }

        private void Showdown_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            {
                _logger.LogInformation("Открытие окна сеттингов");
                _navigationService.NavigateTo<Settings>();
            }
        }

        private void DiceRoller_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogInformation("Открытие окна бросков");
            _navigationService.NavigateTo<DiceRoller>();
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            // Остановить анимацию GIF
            if (CampGifImage != null)
            {
                ImageBehavior.SetAnimatedSource(CampGifImage, null);
            }
        }
    }
}