using Microsoft.Extensions.DependencyInjection;
using NRI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Windows;
public interface INavigationService
{
    void NavigateToPage<T>() where T : Page;
    void ShowWindow<T>() where T : Window;
    void CloseCurrentWindow();
}

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void NavigateToPage<T>() where T : Page
    {
        var page = _serviceProvider.GetRequiredService<T>();
        if (Application.Current.MainWindow is MainWindow mainWindow &&
            mainWindow.MainFrame != null)
        {
            mainWindow.MainFrame.Navigate(page);
        }
    }

    public void ShowWindow<T>() where T : Window
    {
        var window = _serviceProvider.GetRequiredService<T>();
        window.Show();

        if (Application.Current.MainWindow != window)
        {
            Application.Current.MainWindow?.Close();
            Application.Current.MainWindow = window;
        }
    }

    public void CloseCurrentWindow()
    {
        Application.Current.MainWindow?.Close();
    }
}