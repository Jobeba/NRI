﻿using NRI.Kasyanov_NRIDataSetTableAdapters;
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
    /// Логика взаимодействия для Projects.xaml
    /// </summary>
    public partial class Projects : Window
    {
        private NRI.Kasyanov_NRIDataSetTableAdapters.ProjectsTableAdapter _ProjectsTableAdapter;
        private NRI.Kasyanov_NRIDataSet Kasyanov_NRIDataSet;

        public Projects()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            NRI.Kasyanov_NRIDataSet kasyanov_NRIDataSet = ((NRI.Kasyanov_NRIDataSet)(this.FindResource("kasyanov_NRIDataSet")));
            // Загрузить данные в таблицу Projects. Можно изменить этот код как требуется.
            NRI.Kasyanov_NRIDataSetTableAdapters.ProjectsTableAdapter kasyanov_NRIDataSetProjectsTableAdapter = new NRI.Kasyanov_NRIDataSetTableAdapters.ProjectsTableAdapter();
            kasyanov_NRIDataSetProjectsTableAdapter.Fill(kasyanov_NRIDataSet.Projects);
            System.Windows.Data.CollectionViewSource projectsViewSource = ((System.Windows.Data.CollectionViewSource)(this.FindResource("projectsViewSource")));
            projectsViewSource.View.MoveCurrentToFirst();

            Kasyanov_NRIDataSet = ((NRI.Kasyanov_NRIDataSet)(this.FindResource("kasyanov_NRIDataSet")));
            _ProjectsTableAdapter = new NRI.Kasyanov_NRIDataSetTableAdapters.ProjectsTableAdapter();
            _ProjectsTableAdapter.Fill(Kasyanov_NRIDataSet.Projects);
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

        private void Exist_click(object sender, RoutedEventArgs e)
        {
            Registrasya exist = new Registrasya();
            exist.Show();
            this.Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Сохранить изменения в БД
                _ProjectsTableAdapter.Update(Kasyanov_NRIDataSet.Projects);
                MessageBox.Show("Данные сохранены!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                _ProjectsTableAdapter.Fill(Kasyanov_NRIDataSet.Projects); // Обновляем таблицу для отображения последних изменений

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
           
        }
        protected override void OnClosed(EventArgs e)
        {
            long totalMemory = GC.GetTotalMemory(false);

            base.OnClosed(e);
            GC.Collect(1, GCCollectionMode.Forced);
            GC.WaitForPendingFinalizers();
        }
        ~Projects()
        {
            InitializeComponent();
        }
    }
}
