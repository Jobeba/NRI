using Microsoft.Extensions.DependencyInjection;
using NRI.Pages;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace NRI.Controls
{
    public partial class OrganizerWindowControl : UserControl
    {

        public OrganizerWindowControl(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            BaseControl.Loaded += OnLoaded;
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
            BaseControl.ToggleMenu(!(e.Content is ProjectsPage));
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Данные организатора сохранены", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите удалить этого организатора?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                MessageBox.Show("Организатор удален", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
