using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NRI;
using NRI.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

public partial class EventsViewModel : ObservableObject, IDisposable
{
    private bool _disposed = false;
    private DbContext _context;

    public ObservableCollection<Event> Events { get; internal set; }

    // Реализация IDisposable
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context?.Dispose(); // Освобождаем DbContext
            }
            _disposed = true;
        }
    }
}