using NRI.Data;
using NRI.DB;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System;
using NRI.Classes;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Linq;

public partial class EventsViewModel : INotifyPropertyChanged
{
    private readonly AppDbContext _dbContext;
    private readonly User _currentUser;

    public ObservableCollection<Event> UpcomingEvents { get; } = new ObservableCollection<Event>();
    public ObservableCollection<Event> MyEvents { get; } = new ObservableCollection<Event>();
    public ObservableCollection<EventParticipant> EventParticipants { get; } = new ObservableCollection<EventParticipant>();

    public Event SelectedEvent { get; set; }

    public ICommand LoadEventsCommand { get; }
    public ICommand CreateEventCommand { get; }
    public ICommand JoinEventCommand { get; }
    public ICommand LeaveEventCommand { get; }
    public ICommand ShowParticipantsCommand { get; }

    public EventsViewModel(AppDbContext dbContext, User currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;

        LoadEventsCommand = new RelayCommand(LoadEvents);
        CreateEventCommand = new RelayCommand(CreateEvent);
        JoinEventCommand = new RelayCommand(JoinEvent);
        LeaveEventCommand = new RelayCommand(LeaveEvent);
        ShowParticipantsCommand = new RelayCommand(ShowParticipants);

        LoadEvents();
    }

    private async void LoadEvents()
    {
        var events = await _dbContext.Events
            .Include(e => e.OrganizerID)
            .Where(e => e.EventDate > DateTime.Now)
            .OrderBy(e => e.EventDate)
            .ToListAsync();

        UpcomingEvents.Clear();
        foreach (var e in events)
        {
            UpcomingEvents.Add(e);
        }

        var myEvents = await _dbContext.EventParticipants
            .Include(ep => ep.Event).ThenInclude(e => e.OrganizerID)
            .Where(ep => ep.UserID == _currentUser.Id)
            .Select(ep => ep.Event)
            .ToListAsync();

        MyEvents.Clear();
        foreach (var e in myEvents)
        {
            MyEvents.Add(e);
        }
    }

    private async void CreateEvent()
    {
        var newEvent = new Event
        {
            EventName = "Новое мероприятие",
            Description = "Описание мероприятия",
            OrganizerID = _currentUser.Id,
            MaxParticipants = 6,
            EventDate = DateTime.Now.AddDays(7)
        };

        _dbContext.Events.Add(newEvent);
        await _dbContext.SaveChangesAsync();

        LoadEvents();
    }

    private async void JoinEvent()
    {
        if (SelectedEvent == null) return;

        var participant = new EventParticipant
        {
            EventID = SelectedEvent.EventID,
            UserID = _currentUser.Id,

        };

        _dbContext.EventParticipants.Add(participant);
        await _dbContext.SaveChangesAsync();

        LoadEvents();
    }

    private async void LeaveEvent()
    {
        if (SelectedEvent == null) return;

        var participant = await _dbContext.EventParticipants
            .FirstOrDefaultAsync(ep => ep.EventID == SelectedEvent.EventID && ep.UserID == _currentUser.Id);

        if (participant != null)
        {
            _dbContext.EventParticipants.Remove(participant);
            await _dbContext.SaveChangesAsync();
        }

        LoadEvents();
    }

    private async void ShowParticipants()
    {
        if (SelectedEvent == null) return;

        var participants = await _dbContext.EventParticipants
            .Include(ep => ep.User)

            .Where(ep => ep.EventID == SelectedEvent.EventID)
            .ToListAsync();

        EventParticipants.Clear();
        foreach (var p in participants)
        {
            EventParticipants.Add(p);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}