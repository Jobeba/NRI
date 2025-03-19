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
        private NRI.Kasyanov_NRIDataSetTableAdapters.SettingTableAdapter _SettingTableAdapter;
        private NRI.Kasyanov_NRIDataSet Kasyanov_NRIDataSet;
        public Settings()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {



            NRI.Kasyanov_NRIDataSet kasyanov_NRIDataSet = ((NRI.Kasyanov_NRIDataSet)(this.FindResource("kasyanov_NRIDataSet")));
            // Загрузить данные в таблицу Setting. Можно изменить этот код как требуется.
            NRI.Kasyanov_NRIDataSetTableAdapters.SettingTableAdapter kasyanov_NRIDataSetSettingTableAdapter = new NRI.Kasyanov_NRIDataSetTableAdapters.SettingTableAdapter();
            kasyanov_NRIDataSetSettingTableAdapter.Fill(kasyanov_NRIDataSet.Setting);
            System.Windows.Data.CollectionViewSource settingViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("settingViewSource")));
            settingViewSource.View.MoveCurrentToFirst();

            Kasyanov_NRIDataSet = ((NRI.Kasyanov_NRIDataSet)(this.FindResource("kasyanov_NRIDataSet")));
            _SettingTableAdapter = new NRI.Kasyanov_NRIDataSetTableAdapters.SettingTableAdapter();
            _SettingTableAdapter.Fill(Kasyanov_NRIDataSet.Setting);
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
            MainWindow main = new MainWindow();
            main.Show();
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
            try
            {
                // Сохранить изменения в БД
                _SettingTableAdapter.Update(Kasyanov_NRIDataSet.Setting);
                MessageBox.Show("Данные сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                _SettingTableAdapter.Fill(Kasyanov_NRIDataSet.Setting); // Обновляем таблицу для отображения последних изменений

            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); // Обработка ошибок
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Общая ошибка сохранения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
