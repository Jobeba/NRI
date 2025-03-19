using Microsoft.Extensions.DependencyInjection;
using NRI.Kasyanov_NRIDataSetTableAdapters;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NRI
{
    /// <summary>
    /// Логика взаимодействия для Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        private NRI.Kasyanov_NRIDataSet Kasyanov_NRIDataSet;
        public Settings()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            NRI.Kasyanov_NRIDataSet kasyanov_NRIDataSet = ((NRI.Kasyanov_NRIDataSet)(this.FindResource("kasyanov_NRIDataSet")));
            // Загрузить данные в таблицу Settings. Можно изменить этот код как требуется.
            NRI.Kasyanov_NRIDataSetTableAdapters.SettingsTableAdapter kasyanov_NRIDataSetSettingsTableAdapter = new NRI.Kasyanov_NRIDataSetTableAdapters.SettingsTableAdapter();
            kasyanov_NRIDataSetSettingsTableAdapter.Fill(kasyanov_NRIDataSet.Settings);
            System.Windows.Data.CollectionViewSource settingsViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("settingsViewSource")));
            settingsViewSource.View.MoveCurrentToFirst();
        }

        private void Sorting_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void Change_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Add_click(object sender, RoutedEventArgs e)
        {

        }

        private void MainMenu_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = App.ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
            this.Close();
        }

        private void Showdown_click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void DiceRoller_Click(object sender, RoutedEventArgs e)
        {
            DiceRoller dice = new DiceRoller();
            dice.Show();
            this.Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void settingDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Exist_click(object sender, RoutedEventArgs e)
        {
            Registrasya settings = new Registrasya();
            settings.Show();
            this.Close();
        }
        protected override void OnClosed(EventArgs e)
        {
            long totalMemory = GC.GetTotalMemory(false);

            base.OnClosed(e);
            GC.Collect(1, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
        }

        ~Settings()
        {
            InitializeComponent();
        }
    }
}
