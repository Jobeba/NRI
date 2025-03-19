using NRI.Properties;
using System;
using System.Windows;

namespace NRI
{
    /// <summary>
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }
        private void Projects_Click(object sender, RoutedEventArgs e)
        {
            Projects main = new Projects();
            main.Show();
            this.Close();
        }
        private void Players_Click(object sender, RoutedEventArgs e)
        {
            Player main = new Player();
            main.Show();
            this.Close();
        }
        private void Staff_Click(object sender, RoutedEventArgs e)
        {
            staff main = new staff();
            main.Show();
            this.Close();
        }
        private void Reviews_Click(object sender, RoutedEventArgs e)
        {
            Reviews main = new Reviews();
            main.Show();
            this.Close();
        }

        private void Showdown_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            Settings main = new Settings();
            main.Show();
            this.Close();
        }

        private void DiceRoller_Click(object sender, RoutedEventArgs e)
        {
            DiceRoller main = new DiceRoller();
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
        ~MainWindow()
        {
            InitializeComponent();
        }
    }
 

}
