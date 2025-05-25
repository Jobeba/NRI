using System.Windows;
using System.Windows.Controls;
using NRI.Pages; // Добавьте using для страниц

namespace NRI.Pages
{
    public partial class PlayerPage : Page
    {
        public PlayerPage()
        {
            InitializeComponent();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Данные игрока сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите удалить этого игрока?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                MessageBox.Show("Игрок удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Role_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MainMenu_click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.LoadContentBasedOnRole(); 
            }
        }


        private void Showdown_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
