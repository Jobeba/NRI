using NRI.Pages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace NRI.Controls
{
    public partial class PlayerWindowControl : BaseWindowControl  // Наследуемся от BaseWindowControl
    {
        public PlayerWindowControl() : base()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        public PlayerWindowControl(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                mainWindow.MainFrame.Navigated += OnContentNavigated;
            }
        }

        private void OnContentNavigated(object sender, NavigationEventArgs e)
        {
            ToggleMenu(!(e.Content is ProjectsPage));  // Вызываем метод базового класса
        }
    }
}
