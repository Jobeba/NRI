using NRI.Kasyanov_NRIDataSetTableAdapters;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System;
using System.Windows;

namespace NRI
{
    /// <summary>
    /// Логика взаимодействия для sotr.xaml
    /// </summary>
    public partial class staff : Window
    {
        private NRI.Kasyanov_NRIDataSetTableAdapters.StaffTableAdapter _StaffTableAdapter;
        private NRI.Kasyanov_NRIDataSet Kasyanov_NRIDataSet;
        public staff()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            NRI.Kasyanov_NRIDataSet kasyanov_NRIDataSet = ((NRI.Kasyanov_NRIDataSet)(this.FindResource("kasyanov_NRIDataSet")));
            // Загрузить данные в таблицу Staff. Можно изменить этот код как требуется.
            NRI.Kasyanov_NRIDataSetTableAdapters.StaffTableAdapter kasyanov_NRIDataSetStaffTableAdapter = new NRI.Kasyanov_NRIDataSetTableAdapters.StaffTableAdapter();
            kasyanov_NRIDataSetStaffTableAdapter.Fill(kasyanov_NRIDataSet.Staff);
            System.Windows.Data.CollectionViewSource staffViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("staffViewSource")));
            staffViewSource.View.MoveCurrentToFirst();


            Kasyanov_NRIDataSet = ((NRI.Kasyanov_NRIDataSet)(this.FindResource("kasyanov_NRIDataSet")));
            _StaffTableAdapter = new NRI.Kasyanov_NRIDataSetTableAdapters.StaffTableAdapter();
            _StaffTableAdapter.Fill(Kasyanov_NRIDataSet.Staff);

        }
        private void Delete_Click(object sender, RoutedEventArgs e)
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

        private void DiceRoller_click(object sender, RoutedEventArgs e)
        {
            DiceRoller dice = new DiceRoller();
            dice.Show();
            this.Hide();
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Сохранить изменения в БД
                _StaffTableAdapter.Update(Kasyanov_NRIDataSet.Staff);
                MessageBox.Show("Данные сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                _StaffTableAdapter.Fill(Kasyanov_NRIDataSet.Staff); // Обновляем таблицу для отображения последних изменений
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
    }
}
