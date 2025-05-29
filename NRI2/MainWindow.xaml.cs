using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NRI.Classes;
using NRI.Controls;
using NRI.Helpers;
using NRI.Pages;
using NRI.Services;
using NRI.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;
using System.Security.Claims;
using NRI.Data;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;

namespace NRI
{
    public partial class MainWindow : Window
    {
        public RelayCommand ToggleMenuCommand { get; }

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MainWindow> _logger;
        private readonly INavigationService _navigationService;
        private readonly JwtService _jwtService;

        private bool _isMenuExpanded = false;
         private Grid _contentGrid;
        private readonly TimeSpan _animationDuration = TimeSpan.FromMilliseconds(400);
        private ScaleTransform _backgroundScale;
        private BlurEffect _backgroundBlur;
        private ScaleTransform _contentScale;
        private PackIcon _toggleIcon;
        private bool _isWindowTransitionInProgress;

        public MainWindow(
            IServiceProvider serviceProvider,
            ILogger<MainWindow> logger,
            INavigationService navigationService,
            JwtService jwtService,
            UserActivityService activityService)

        {
            InitializeComponent();

            

            Loaded += MainWindow_Loaded;

            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));

            DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>();

            ToggleMenuCommand = new RelayCommand(() => ToggleMenuForCurrentContent());

            // Проверка параметров
            if (serviceProvider == null) throw new ArgumentNullException(nameof(serviceProvider));
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (navigationService == null) throw new ArgumentNullException(nameof(navigationService));

            if (MainFrame == null)
            {
                _logger.LogError("MainFrame не инициализирован!");
                throw new NullReferenceException("MainFrame не найден в XAML");
            }

            // Инициализация трансформаций фона
            var bgTransform = BackgroundImage.RenderTransform;
            if (bgTransform is ScaleTransform scaleTransform)
            {
                _backgroundScale = scaleTransform;
            }
            else
            {
                _backgroundScale = new ScaleTransform(1.05, 1.05);
                BackgroundImage.RenderTransform = _backgroundScale;
            }

            _backgroundBlur = (BlurEffect)BackgroundImage.Effect ?? new BlurEffect { Radius = 4 };

            _contentGrid = new Grid
            {
                Name = "ContentGrid",
                Opacity = 0,
                Visibility = Visibility.Collapsed,
                RenderTransform = new ScaleTransform(0.9, 0.9)
            };
            MainGrid.Children.Add(_contentGrid);


            InitializeTransforms();

            // Гарантируем, что трансформации привязаны
            BackgroundImage.RenderTransform = _backgroundScale;
            BackgroundImage.Effect = _backgroundBlur;

            LoadContentBasedOnRole();

            DataContext = _serviceProvider.GetRequiredService<MainWindowViewModel>();

            activityService.StartTracking();

            _logger.LogInformation("Главное меню запустилось");

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Application.Current.Properties.Contains("JwtToken") &&
                    Application.Current.Properties["JwtToken"] is string token)
                {
                    var principal = _jwtService.ValidateToken(token);
                    if (principal != null && DataContext is MainWindowViewModel vm)
                    {
                        // Получаем полные данные пользователя из базы
                        using (var context = new AppDbContext())
                        {
                            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                            if (int.TryParse(userId, out var id))
                            {
                                var user = context.Users
                                    .Include(u => u.UserRoles)
                                    .ThenInclude(ur => ur.Role)
                                    .FirstOrDefault(u => u.Id == id);

                                if (user != null)
                                {
                                    // Получаем роли пользователя
                                    var roles = user.UserRoles?.Select(ur => ur.Role?.RoleName).ToList()
                                        ?? new List<string> { "Игрок" };

                                    vm.CurrentUser = user;
                                    vm.SetUserRoles(roles);
                                    LoadContentBasedOnRole();
                                    return;
                                }
                            }
                        }
                    }
                }

                _logger.LogWarning("Пользователь не аутентифицирован");
                ShowAuthWindow();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке главного окна");
                ShowAuthWindow();
            }
        }

        private void CleanupResources()
        {
            // Останавливаем анимации
            ImageBehavior.SetAnimatedSource(BackgroundImage, null);

            // Очищаем подписки
            Loaded -= MainWindow_Loaded;

            // Освобождаем графические ресурсы
            BackgroundImage.Source = null;

            // Очищаем DataContext
            DataContext = null;

            // Принудительный сбор мусора
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        private void ToggleMenuForCurrentContent()
        {
            if (MainFrame.Content is BaseWindowControl windowControl)
            {
                windowControl.ToggleMenu(true);
            }
        }

        private void ShowAuthWindow()
        {
            var authWindow = _serviceProvider.GetRequiredService<Autorizatsaya>();
            authWindow.Show();
            this.Close();
        }
        private void Logout()
        {
            // Очищаем токен
            if (Application.Current.Properties.Contains("JwtToken"))
            {
                Application.Current.Properties.Remove("JwtToken");
            }

            // Показываем окно авторизации
            var authWindow = _serviceProvider.GetRequiredService<Autorizatsaya>();
            authWindow.Show();

            // Закрываем текущее окно
            this.Close();
        }

        private void Showdown_Click(object sender, RoutedEventArgs e)
        {
            Logout();
        }


        private async void MainMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isMenuExpanded)
            {
                await CollapseMenuAsync();
            }
            else
            {
                await ExpandMenuAsync();
            }
            _isMenuExpanded = !_isMenuExpanded;
        }
        private async Task ExpandMenuAsync()
        {
            _contentGrid.Visibility = Visibility.Visible;
            _contentGrid.Opacity = 0;

            var opacityAnim = new DoubleAnimation(1, _animationDuration);
            var scaleAnim = new DoubleAnimation(1, _animationDuration);

            _contentGrid.BeginAnimation(OpacityProperty, opacityAnim);
            ((ScaleTransform)_contentGrid.RenderTransform)
                .BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            ((ScaleTransform)_contentGrid.RenderTransform)
                .BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);

            await Task.Delay(_animationDuration);
        }

        public void SetUserRoles(List<string> roles)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.SetUserRoles(roles);
                LoadContentBasedOnRole(); // Перезагружаем контент после установки ролей
            }
        }
        private async Task CollapseMenuAsync()
        {
            var opacityAnim = new DoubleAnimation(0, _animationDuration);
            var scaleAnim = new DoubleAnimation(0.9, _animationDuration);

            _contentGrid.BeginAnimation(OpacityProperty, opacityAnim);
            ((ScaleTransform)_contentGrid.RenderTransform)
                .BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnim);
            ((ScaleTransform)_contentGrid.RenderTransform)
                .BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnim);

            await Task.Delay(_animationDuration);
            _contentGrid.Visibility = Visibility.Collapsed;
        }

        public void LoadContentBasedOnRole()
        {
            try
            {
                var vm = DataContext as MainWindowViewModel;
                if (vm == null)
                {
                    _logger.LogWarning("ViewModel не инициализирована");
                    return;
                }

                var highestRole = vm.HighestRole;
                _logger.LogInformation($"Определена наивысшая роль: {highestRole}");

                switch (highestRole)
                {
                    case "Администратор":
                        MainFrame.Content = _serviceProvider.GetRequiredService<AdminWindowControl>();
                        break;
                    case "Организатор":
                        MainFrame.Content = _serviceProvider.GetRequiredService<OrganizerWindowControl>();
                        break;
                    default:
                        MainFrame.Content = _serviceProvider.GetRequiredService<PlayerWindowControl>();
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки контента");
                MainFrame.Content = new Controls.ErrorPage($"Ошибка загрузки: {ex.Message}");
            }
        }      

        private void NavigateToProjects(object sender, RoutedEventArgs e)
        {
            try
            {
                var projectsPage = _serviceProvider.GetRequiredService<ProjectsPage>();
                MainFrame.Navigate(projectsPage);

                // Обновляем состояние меню для текущего контрола
                if (MainFrame.Content is BaseWindowControl currentControl)
                {
                    currentControl.ToggleMenu(false);
                }

                if (_isMenuExpanded)
                {
                    CollapseMenuAsync().ConfigureAwait(false);
                    _isMenuExpanded = false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка навигации");
                MessageBox.Show("Ошибка навигации", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void ToggleFullscreen_Click(object sender, RoutedEventArgs e)
        {
            if (_isWindowTransitionInProgress) return;
            _isWindowTransitionInProgress = true;

            try
            {
                if (_contentScale == null || _backgroundScale == null || _backgroundBlur == null)
                {
                    InitializeTransforms();
                }

                await AnimationHelper.ToggleFullscreenMode(
                    this,
                    _contentScale,
                    _backgroundScale,
                    _backgroundBlur,
                    _toggleIcon,
                    _animationDuration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка переключения режима окна");
            }
            finally
            {
                _isWindowTransitionInProgress = false;
            }
        }

        private void InitializeTransforms()
        {
            try
            {
                _logger.LogInformation("Инициализация трансформаций");

                // 1. Инициализация трансформации фона (BackgroundImage)
                var bgTransform = BackgroundImage.RenderTransform;
                if (bgTransform is ScaleTransform scaleTransform)
                {
                    _backgroundScale = scaleTransform;
                }
                else
                {
                    _backgroundScale = new ScaleTransform(1.05, 1.05);
                    BackgroundImage.RenderTransform = _backgroundScale;
                }

                // 2. Инициализация эффекта размытия фона
                _backgroundBlur = BackgroundImage.Effect as BlurEffect ?? new BlurEffect { Radius = 4 };
                BackgroundImage.Effect = _backgroundBlur;

                // 3. Инициализация трансформации контента (ContentGrid)
                var contentTransform = _contentGrid.RenderTransform;
                TransformGroup contentTransformGroup = contentTransform as TransformGroup;

                if (contentTransformGroup != null)
                {
                    _contentScale = contentTransformGroup.Children.OfType<ScaleTransform>().FirstOrDefault();
                }
                else if (contentTransform is ScaleTransform directScaleTransform)
                {
                    _contentScale = directScaleTransform;
                }

                if (_contentScale == null)
                {
                    _contentScale = new ScaleTransform(1.0, 1.0);
                    _contentGrid.RenderTransform = new TransformGroup
                    {
                        Children = new TransformCollection { _contentScale }
                    };
                    contentTransformGroup = _contentGrid.RenderTransform as TransformGroup;
                }

                // 4. Проверка и добавление трансформации если нужно
                if (contentTransformGroup != null && !contentTransformGroup.Children.Contains(_contentScale))
                {
                    contentTransformGroup.Children.Add(_contentScale);
                }

                _logger.LogInformation("Трансформации успешно инициализированы");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка инициализации трансформаций");
                throw; // Можно заменить на восстановление значений по умолчанию
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var closingAnimation = new DoubleAnimation(0, TimeSpan.FromSeconds(0.2));
            closingAnimation.Completed += (s, _) => Close();
            BeginAnimation(OpacityProperty, closingAnimation);
        }

        private void InitializeBackground()
        {
            try
            {
                // Для анимированного GIF используем ImageBehavior
                var image = new BitmapImage(new Uri("pack://application:,,,/Gifs/background.gif"));
                ImageBehavior.SetAnimatedSource(BackgroundImage, image);
                ImageBehavior.SetRepeatBehavior(BackgroundImage, RepeatBehavior.Forever);

                // Настройка эффектов
                BackgroundImage.Stretch = Stretch.UniformToFill;
                _backgroundBlur.Radius = 4;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки фонового изображения");
                // Запасной вариант
                BackgroundImage.Source = new BitmapImage(new Uri("pack://application:,,,/Images/fallback-background.jpg"));
            }
        }

        private void DiceRoller_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MainFrame.Content is BaseWindowControl currentControl)
                {
                    currentControl.NavigateToDiceRoller();
                }
                else
                {
                    var diceRollerPage = _serviceProvider.GetRequiredService<DiceRollerPage>();
                    MainFrame.Navigate(diceRollerPage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка навигации к DiceRoller");
                MessageBox.Show("Ошибка открытия бросков кубиков", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            ImageBehavior.SetAnimatedSource(BackgroundImage, null);
        }

        private void CloseApp(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void NavigateToHome(object sender, RoutedEventArgs e)
        {
            if (MainFrame.Content is IRoleNavigation navigator)
            {
                navigator.NavigateToHome();
            }
        }

        private void MinimizeWindow(object sender, RoutedEventArgs e) =>
                    WindowState = WindowState.Minimized;

        private void ToggleMaximize(object sender, RoutedEventArgs e) =>
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;       
    }
}
