using Microsoft.Extensions.DependencyInjection;
using NRI.Data;
using NRI.Pages;
using NRI.Services;
using NRI.ViewModels;
using System;
using System.Data.Entity.Infrastructure;
using System.Windows;
using System.Windows.Controls;

namespace NRI.Controls
{
    public partial class AdminWindowControl : UserControl, IRoleNavigation
    {
        private readonly AdminViewModel _viewModel;
        public AdminWindowControl()
        {
            InitializeComponent();

            if (App.ServiceProvider != null)
            {
                var userService = App.ServiceProvider.GetRequiredService<IUserService>();
                var gameSystemService = App.ServiceProvider.GetRequiredService<IGameSystemService>();
                DataContext = new AdminViewModel(userService, gameSystemService);
            }
        }

        public void NavigateToHome()
        {
            if (DataContext is AdminViewModel vm)
            {
                vm.ShowUsersSection();
            }
        }


        public void NavigateToDiceRoller()
        {
            try
            {
                if (App.ServiceProvider != null &&
                    Application.Current.MainWindow is MainWindow mainWindow)
                {
                    var diceRollerPage = App.ServiceProvider.GetRequiredService<DiceRollerPage>();
                    mainWindow.MainFrame.Navigate(diceRollerPage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка навигации: {ex.Message}",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        public void ToggleMenu(bool isVisible)
        {
            // Реализация переключения меню
        }
    }
}
