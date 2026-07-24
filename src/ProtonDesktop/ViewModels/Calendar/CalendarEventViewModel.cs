using CommunityToolkit.Mvvm.ComponentModel;
using ProtonDesktop.Core.Models;

namespace ProtonDesktop.ViewModels.Calendar;

public partial class CalendarEventViewModel : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _uid = string.Empty;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string? _location;

    [ObservableProperty]
    private DateTime _startUtc;

    [ObservableProperty]
    private DateTime _endUtc;

    [ObservableProperty]
    private bool _isAllDay;

    [ObservableProperty]
    private string? _color;

    [ObservableProperty]
    private int _calendarId;

    public CalendarEventViewModel(CalendarEvent calendarEvent)
    {
        Id = calendarEvent.Id;
        Uid = calendarEvent.Uid;
        Title = calendarEvent.Title;
        Description = calendarEvent.Description;
        Location = calendarEvent.Location;
        StartUtc = calendarEvent.StartUtc;
        EndUtc = calendarEvent.EndUtc;
        IsAllDay = calendarEvent.IsAllDay;
        CalendarId = calendarEvent.CalendarId;
    }

    public string StartTime => StartUtc.ToLocalTime().ToString("HH:mm");
    public string EndTime => EndUtc.ToLocalTime().ToString("HH:mm");
    public string Date => StartUtc.ToLocalTime().ToString("MMM d, yyyy");
    public string DateTimeRange => IsAllDay ? "All day" : $"{StartTime} - {EndTime}";
}
