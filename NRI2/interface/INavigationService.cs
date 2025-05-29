using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using NRI;

public interface INavigationService
{
    void NavigateToPage<T>(object parameter = null) where T : Page;
    void ShowWindow<T>(object parameter = null) where T : Window;
    void CloseCurrentWindow();
    void GoBack();
}

public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private Window _currentWindow;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void NavigateToPage<T>(object parameter = null) where T : Page
    {
        var page = _serviceProvider.GetService<T>();
        if (page == null)
            throw new InvalidOperationException($"Page {typeof(T).Name} is not registered in DI container");

        if (Application.Current.MainWindow is MainWindow mainWindow && mainWindow.MainFrame != null)
        {
            if (parameter != null && page is IParameterReceiver parameterReceiver)
            {
                parameterReceiver.ReceiveParameter(parameter);
            }
            mainWindow.MainFrame.Navigate(page);
        }
    }

    public void ShowWindow<T>(object parameter = null) where T : Window
    {
        var window = _serviceProvider.GetService<T>();
        if (window == null)
            throw new InvalidOperationException($"Window {typeof(T).Name} is not registered in DI container");

        if (parameter != null && window is IParameterReceiver parameterReceiver)
        {
            parameterReceiver.ReceiveParameter(parameter);
        }

        window.Show();
        _currentWindow = window;

        if (Application.Current.MainWindow != window)
        {
            Application.Current.MainWindow?.Close();
            Application.Current.MainWindow = window;
        }
    }

    public void CloseCurrentWindow()
    {
        _currentWindow?.Close();
    }

    public void GoBack()
    {
        if (Application.Current.MainWindow is MainWindow mainWindow &&
            mainWindow.MainFrame != null &&
            mainWindow.MainFrame.CanGoBack)
        {
            mainWindow.MainFrame.GoBack();
        }
    }
}

public interface IParameterReceiver
{
    void ReceiveParameter(object parameter);
}
