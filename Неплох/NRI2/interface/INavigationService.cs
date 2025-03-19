using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
public interface INavigationService
    {
        void NavigateTo<TWindow>() where TWindow : Window;
    }

    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _serviceProvider;

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

    public void NavigateTo<TWindow>() where TWindow : Window
    {
        try
        {
            var window = _serviceProvider.GetRequiredService<TWindow>();
            window.Show();

            // Закрыть текущее главное окно, если оно существует
            if (Application.Current.MainWindow != null && Application.Current.MainWindow != window)
            {
                Application.Current.MainWindow.Close();
            }

            // Назначить новое окно как главное
            Application.Current.MainWindow = window;
        }
        catch (Exception ex)
        {
            // Логирование ошибки
            Console.WriteLine($"Ошибка при навигации: {ex.Message}");
            throw;
        }
    }
}
