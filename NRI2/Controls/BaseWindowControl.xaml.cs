using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Media.Animation;
using System;
using Microsoft.Extensions.Logging;
using NRI.Pages;
using GalaSoft.MvvmLight.Views;

namespace NRI.Controls
{
    public partial class BaseWindowControl : UserControl, IRoleNavigation
    {

        public static readonly DependencyProperty SideMenuContentProperty =
            DependencyProperty.Register(
                "SideMenuContent",
                typeof(object),
                typeof(BaseWindowControl));

        public static readonly DependencyProperty MainContentProperty =
            DependencyProperty.Register(
                "MainContent",
                typeof(object),
                typeof(BaseWindowControl));

        protected readonly IServiceProvider ServiceProvider;

        public object SideMenuContent
        {
            get => GetValue(SideMenuContentProperty);
            set => SetValue(SideMenuContentProperty, value);
        }

        public object MainContent
        {
            get => GetValue(MainContentProperty);
            set => SetValue(MainContentProperty, value);
        }

        public BaseWindowControl() : this(null) { }

        public BaseWindowControl(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            InitializeComponent();
        }

        public virtual void ToggleMenu(bool isVisible)
        {
            if (this.FindName("MenuColumn") is ColumnDefinition menuColumn)
            {
                var animation = new DoubleAnimation
                {
                    From = isVisible ? 0 : 180,
                    To = isVisible ? 180 : 0,
                    Duration = TimeSpan.FromSeconds(0.3)
                };

                menuColumn.BeginAnimation(ColumnDefinition.WidthProperty, animation);
            }
        }


        public virtual void NavigateToHome()
        {
            try
            {
                var navigationService = ServiceProvider.GetRequiredService<INavigationService>();
                var mainPage = ServiceProvider.GetRequiredService<MainMenuPage>(); // Убедитесь, что MainPage унаследован от System.Windows.Controls.Page

                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.MainFrame.Navigate(mainPage);
                }
            }
            catch (Exception ex)
            {
                var logger = ServiceProvider.GetRequiredService<ILogger<BaseWindowControl>>();
                logger.LogError(ex, "Ошибка навигации");
                MessageBox.Show("Ошибка навигации", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public virtual void NavigateToDiceRoller()
        {
            try
            {
                if (Application.Current.MainWindow is MainWindow mainWindow)
                {
                    var diceRollerPage = ServiceProvider.GetRequiredService<DiceRollerPage>();
                    mainWindow.MainFrame.Navigate(diceRollerPage);
                }
            }
            catch (Exception ex)
            {
                var logger = ServiceProvider.GetRequiredService<ILogger<BaseWindowControl>>();
                logger.LogError(ex, "Ошибка навигации к DiceRoller");
                MessageBox.Show("Ошибка открытия бросков кубиков", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
