using Microsoft.Extensions.DependencyInjection;
using NRI.Pages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace NRI
{
    public interface IRoleNavigation
    {
        void NavigateToHome();
        void NavigateToDiceRoller();
        void ToggleMenu(bool isVisible);
    }

    public class BaseWindowControl : UserControl, IRoleNavigation
    {
        protected readonly IServiceProvider ServiceProvider;

        public BaseWindowControl()
        {
            // Default constructor
        }

        public BaseWindowControl(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public void ToggleMenu(bool isVisible)
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

        public void NavigateToHome()
        {
            try
            {
                var navigationService = ServiceProvider.GetService<INavigationService>();
                navigationService?.NavigateToPage<MainMenuPage>();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка навигации: " + ex.Message);
            }
        }

        public void NavigateToDiceRoller()
        {
            try
            {
                var navigationService = ServiceProvider.GetService<INavigationService>();
                navigationService?.NavigateToPage<DiceRollerPage>();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка навигации: " + ex.Message);
            }
        }
    }
}
