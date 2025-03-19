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
    /// Логика взаимодействия для Reviews.xaml
    /// </summary>
    public partial class Reviews : Window
    {
        private NRI.Kasyanov_NRIDataSetTableAdapters.ReviewsTableAdapter _ReviewsTableAdapter;
        private NRI.Kasyanov_NRIDataSet Kasyanov_NRIDataSet;
        public Reviews()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Exist_click(object sender, RoutedEventArgs e)
        {
            Registrasya exist = new Registrasya();
            exist.Show();
            this.Close();
        }
        private void Delete_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Сохранить изменения в БД
                _ReviewsTableAdapter.Update(Kasyanov_NRIDataSet.Reviews);
                MessageBox.Show("Данные сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                _ReviewsTableAdapter.Fill(Kasyanov_NRIDataSet.Reviews); // Обновляем таблицу для отображения последних изменений

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
        protected override void OnClosed(EventArgs e)
        {
            long totalMemory = GC.GetTotalMemory(false);

            base.OnClosed(e);
            GC.Collect(1, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
        }
        ~Reviews()
        {
            InitializeComponent();
        }
    }
}
