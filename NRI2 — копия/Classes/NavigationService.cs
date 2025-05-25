using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace NRI.Services
{
    public interface INavigationService
    {
        void NavigateToPage<T>() where T : Page;
        void NavigateToWindow<T>() where T : Window;
        void GoBack();
    }

    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void NavigateToPage<T>() where T : Page
        {
            var page = _serviceProvider.GetRequiredService<T>();
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.MainFrame.Navigate(page);
            }
        }

        public void NavigateToWindow<T>() where T : Window
        {
            var window = _serviceProvider.GetRequiredService<T>();
            window.Show();

            if (Application.Current.MainWindow is Window currentWindow &&
                currentWindow != window &&
                currentWindow is not Autorizatsaya)
            {
                currentWindow.Close();
            }

            Application.Current.MainWindow = window;
        }

        public void GoBack()
        {
            if (Application.Current.MainWindow is MainWindow mainWindow &&
                mainWindow.MainFrame.CanGoBack)
            {
                mainWindow.MainFrame.GoBack();
            }
        }
    }
}
