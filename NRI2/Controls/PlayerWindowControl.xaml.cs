using NRI.Pages;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace NRI.Controls
{
    public partial class PlayerWindowControl : UserControl
    {
        public PlayerWindowControl()
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
            if (this.Content is BaseWindowControl baseControl)
            {
                baseControl.ToggleMenu(!(e.Content is ProjectsPage));
            }
        }
    }
}
