using NRI.Kasyanov_NRIDataSetTableAdapters;
using System.Data.SqlClient;
using System;
using System.Windows;
using System.Data;
using System.Diagnostics;

namespace NRI
{
    /// <summary>
    /// Логика взаимодействия для player.xaml
    /// </summary>
    public partial class Player : Window
    {
        private NRI.Kasyanov_NRIDataSetTableAdapters.ClientTableAdapter _ClientTableAdapter;
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

            Kasyanov_NRIDataSet = ((NRI.Kasyanov_NRIDataSet)(this.FindResource("kasyanov_NRIDataSet")));
            _ClientTableAdapter = new NRI.Kasyanov_NRIDataSetTableAdapters.ClientTableAdapter();
            _ClientTableAdapter.Fill(Kasyanov_NRIDataSet.Client);

            NRI.Kasyanov_NRIDataSet kasyanov_NRIDataSet = ((NRI.Kasyanov_NRIDataSet)(this.FindResource("kasyanov_NRIDataSet")));
            // Загрузить данные в таблицу Client. Можно изменить этот код как требуется.
            NRI.Kasyanov_NRIDataSetTableAdapters.ClientTableAdapter kasyanov_NRIDataSetClientTableAdapter = new NRI.Kasyanov_NRIDataSetTableAdapters.ClientTableAdapter();
            kasyanov_NRIDataSetClientTableAdapter.Fill(kasyanov_NRIDataSet.Client);
            System.Windows.Data.CollectionViewSource clientViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("clientViewSource")));
            clientViewSource.View.MoveCurrentToFirst();
        }

        private void clientDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Сохранить изменения в БД
                _ClientTableAdapter.Update(Kasyanov_NRIDataSet.Client);
                MessageBox.Show("Данные сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                _ClientTableAdapter.Fill(Kasyanov_NRIDataSet.Client); // Обновляем таблицу для отображения последних изменений

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

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("DeleteButton_Click started");
            if (clientDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите запись для удаления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                Debug.WriteLine("DeleteButton_Click finished - no selection");
                return;
            }

            try
            {
                // Получаем выбранную строку (DataRowView)
                DataRowView rowToDelete = (DataRowView)clientDataGrid.SelectedItem;
                Debug.WriteLine($"Deleting Row with ID: {rowToDelete["ID_Клиента"]}");
                // Удаляем строку из DataTable
                rowToDelete.Row.Delete();
                Debug.WriteLine($"Row with ID {rowToDelete["ID_Клиента"]} marked for deletion");
                // Сохраняем изменения (включая удаление)
                _ClientTableAdapter.Update(Kasyanov_NRIDataSet.Client);
                Debug.WriteLine($"Changes (including deletion) committed to DB");
                // Обновляем таблицу
                _ClientTableAdapter.Fill(Kasyanov_NRIDataSet.Client);
                Debug.WriteLine("Data refreshed in dataGrid");

                MessageBox.Show("Запись удалена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                Debug.WriteLine("DeleteButton_Click finished - success");
            }
            catch (SqlException ex)
            {
                Debug.WriteLine($"SQL Exception (Delete): {ex.Message}");
                MessageBox.Show($"Ошибка удаления(SQL): {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"General Exception (Delete): {ex.Message}");
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Debug.WriteLine("DeleteButton_Click finished");
        }

        private void MainMenu_click(object sender, RoutedEventArgs e)
        {
            MainWindow main = new MainWindow();
            main.Show();
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
