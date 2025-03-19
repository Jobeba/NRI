using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NRI.Classes
{
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public void NavigateTo<TWindow>() where TWindow : Window
        {
            var window = _serviceProvider.GetRequiredService<TWindow>();
            window.Show();
            Application.Current.MainWindow?.Close();
            Application.Current.MainWindow = window;
        }
    }
}
