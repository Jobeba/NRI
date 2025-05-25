using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace NRI.Windows
{
    public partial class PDFPreviewWindow : Window
    {
        private readonly string _pdfPath;
        private bool _isLoaded;

        public PDFPreviewWindow(string pdfPath)
        {
            InitializeComponent();
            _pdfPath = pdfPath;

            // Обработчики событий
            Loaded += OnWindowLoaded;
            Closed += OnWindowClosed;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!File.Exists(_pdfPath))
                {
                    MessageBox.Show("Файл для предпросмотра не найден", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                // Задержка для инициализации WebBrowser
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    try
                    {
                        // Используем Microsoft Edge для лучшего отображения
                        PdfViewer.Navigate(new Uri(_pdfPath));
                        _isLoaded = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка загрузки PDF: {ex.Message}", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        Close();
                    }
                }), DispatcherPriority.ApplicationIdle);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void PrintButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isLoaded)
            {
                MessageBox.Show("Документ еще не загружен", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Печатаем PDF
                PrintPdfDocument(_pdfPath);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при печати: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void PrintPdfDocument(string pdfPath)
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = pdfPath,
                        Verb = "print",
                        CreateNoWindow = true,
                        WindowStyle = ProcessWindowStyle.Hidden,
                        UseShellExecute = true
                    }
                };
                process.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при печати: {ex.Message}");
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void OnWindowClosed(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(_pdfPath))
                {
                    var timer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromSeconds(2)
                    };
                    timer.Tick += (s, args) =>
                    {
                        timer.Stop();
                        try { File.Delete(_pdfPath); } catch { }
                    };
                    timer.Start();
                }
            }
            catch { /* Игнорируем ошибки удаления */ }
        }
    }
}
