using NRI.Kasyanov_NRIDataSetTableAdapters;
using System.Data.SqlClient;
using System;
using System.Windows;
using System.Data;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace NRI
{
    /// <summary>
    /// Логика взаимодействия для player.xaml
    /// </summary>
    public partial class Player : Window
    {

        private NRI.Kasyanov_NRIDataSet Kasyanov_NRIDataSet;
        public Player()
        {
            InitializeComponent();
        }
        private void Showdown_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Role_Click(object sender, RoutedEventArgs e)
        {
            DiceRoller Roll = new DiceRoller();
            Roll.Show();
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void clientDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
          
        }

        private void MainMenu_click(object sender, RoutedEventArgs e)
        {
            var mainWindow = App.ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            this.Close();
        }
        protected override void OnClosed(EventArgs e)
        {
            long totalMemory = GC.GetTotalMemory(false);

            base.OnClosed(e);
            GC.Collect(1, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
        }

        ~Player()
        {
            InitializeComponent();
        }
    }
}
