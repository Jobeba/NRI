using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NRI.Classes;
using NRI.Controls;
using NRI.Data;
using NRI.DB;
using NRI.Helpers;
using NRI.Pages;
using NRI.Services;
using NRI.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfAnimatedGif;

namespace NRI
{
    public partial class MainWindow : Window
    {
        public RelayCommand ToggleMenuCommand { get; }

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MainWindow> _logger;
        private readonly INavigationService _navigationService;
        private readonly JwtService _jwtService;

        public ICommand BackToAuthCommand { get; }

        private bool _isMenuExpanded = false;
         private Grid _contentGrid;
        private readonly TimeSpan _animationDuration = TimeSpan.FromMilliseconds(400);
        private ScaleTransform _backgroundScale;
        private ScaleTransform _contentScale;
        private PackIcon _toggleIcon;
        private bool _isWindowTransitionInProgress;

        private DispatcherTimer _bgAnimationTimer;
        private int _currentBgFrame;
        private BitmapSource[] _bgFrames;
        private BitmapImage _gifSource;

        public MainWindow(
            IServiceProvider serviceProvider,
            ILogger<MainWindow> logger,
            INavigationService navigationService,
            JwtService jwtService,
            UserActivityService activityService)

        {
            InitializeComponent();

            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _navigationService = navigationService ??
                throw new ArgumentNullException(nameof(navigationService));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _backgroundScale = BackgroundImage.RenderTransform as ScaleTransform ??
                   new ScaleTransform(1.05, 1.05);

            activityService = activityService ?? throw new ArgumentNullException(nameof(activityService));

            var viewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();
            viewModel.Initialize(_serviceProvider);

            DataContext = viewModel;

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

            }

            _contentGrid = new Grid
            {
                Name = "ContentGrid",
                Opacity = 0,
                Visibility = Visibility.Collapsed,
                RenderTransform = new ScaleTransform(0.9, 0.9)
            };
            MainGrid.Children.Add(_contentGrid);

            InitializeTransforms();

            CleanupResources();

            InitializeBackground();


            activityService.StartTracking();

            _logger.LogInformation("Главное меню запустилось");

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверяем наличие токена
                if (!Application.Current.Properties.Contains("JwtToken"))
                {
                    _logger.LogWarning("JWT токен не найден. Пользователь не аутентифицирован");
                    ShowAuthWindow();
                    return;
                }

                var token = Application.Current.Properties["JwtToken"] as string;
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Пустой JWT токен");
                    ShowAuthWindow();
                    return;
                }

                var principal = _jwtService.ValidateToken(token);
                if (principal == null)
                {
                    _logger.LogWarning("Не удалось валидировать токен");
                    ShowAuthWindow();
                    return;
                }

                if (DataContext is not MainWindowViewModel vm)
                {
                    _logger.LogError("ViewModel не инициализирована");
                    // Пробуем инициализировать ViewModel
                    vm = _serviceProvider.GetRequiredService<MainWindowViewModel>();
                    DataContext = vm;
                }

                // Получаем ID пользователя из токена
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userId, out var id))
                {
                    _logger.LogWarning("Неверный ID пользователя в токене");
                    ShowAuthWindow();
                    return;
                }

                // Загружаем данные пользователя из БД
                using (var context = new AppDbContext())
                {
                    var user = context.Users
                        .Include(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
                        .FirstOrDefault(u => u.Id == id);

                    if (user == null)
                    {
                        _logger.LogWarning($"Пользователь с ID {id} не найден в БД");
                        ShowAuthWindow();
                        return;
                    }

                    // Обновляем активность
                    user.LastActivity = DateTime.UtcNow;
                    context.SaveChanges();

                    // Получаем роли
                    var roles = user.UserRoles?
                        .Select(ur => ur.Role?.RoleName)
                        .Where(r => !string.IsNullOrEmpty(r))
                        .ToList() ?? new List<string>();

                    if (!roles.Any())
                    {
                        _logger.LogWarning("Для пользователя не найдены роли в базе данных");
                        roles = new List<string> { "Игрок" };
                    }

                    // Устанавливаем данные пользователя
                    vm.CurrentUser = user;
                    vm.SetUserRoles(roles);
                    _logger.LogInformation($"Установлены роли: {string.Join(", ", roles)}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке главного окна");
                ShowAuthWindow();
            }
        }

        private void CleanupResources()
        {
            try
            {
                // Останавливаем и освобождаем анимацию
                ImageBehavior.SetAnimatedSource(BackgroundImage, null);

                // Явно освобождаем источник изображения
                if (BackgroundImage.Source is IDisposable disposable)
                {
                    disposable.Dispose();
                }

                BackgroundImage.Source = null;
                BackgroundImage.Effect = null;

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при очистке ресурсов");
            }
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

        private void Showdown_Click(object sender, RoutedEventArgs e)
        {
            Logout();
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
            var anim = new DoubleAnimation(1, TimeSpan.FromMilliseconds(200));
            _contentGrid.BeginAnimation(OpacityProperty, anim);
            await Task.Delay(200);
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
                if (_contentScale == null || _backgroundScale == null)
                {
                    InitializeTransforms();
                }

                await AnimationHelper.ToggleFullscreenMode(
                    this,
                    _contentScale,
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

                }


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
                Dispatcher.Invoke(() =>
                {
                    var uri = new Uri("pack://application:,,,/Gifs/background.gif");
                    var image = new BitmapImage(uri);

                    // Убедитесь, что старое изображение очищено
                    ImageBehavior.SetAnimatedSource(BackgroundImage, null);

                    ImageBehavior.SetAnimatedSource(BackgroundImage, image);
                    ImageBehavior.SetRepeatBehavior(BackgroundImage, new RepeatBehavior(0));

                    // Оптимизация производительности
                    RenderOptions.SetBitmapScalingMode(BackgroundImage, BitmapScalingMode.LowQuality);
                    RenderOptions.SetCachingHint(BackgroundImage, CachingHint.Cache);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки фонового изображения");
                // Фолбэк на статичное изображение
                var fallback = new BitmapImage(new Uri("pack://application:,,,/Images/fallback.jpg"));
                BackgroundImage.Source = fallback;
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
            // Правильное освобождение ресурсов
            ImageBehavior.SetAnimatedSource(BackgroundImage, null);
            BackgroundImage.Source = null;

            _bgAnimationTimer?.Stop();
            _gifSource = null;
            _bgFrames = null;

            base.OnClosed(e);

            // Принудительная очистка памяти
            Dispatcher.Invoke(() =>
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }, DispatcherPriority.Background);
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

        private void Button_Click(object sender, RoutedEventArgs e)
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
    }
}
