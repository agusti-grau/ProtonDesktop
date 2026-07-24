using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonDesktop.Core.Interfaces;
using ProtonDesktop.Core.Models;
using Serilog;
using System.Collections.ObjectModel;

namespace ProtonDesktop.ViewModels.Calendar;

public partial class EventEditorViewModel : ObservableObject
{
    private readonly ICalendarRepository _calendarRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger _logger;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string? _location;

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today;

    [ObservableProperty]
    private TimeSpan _startTime = new TimeSpan(9, 0, 0);

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today;

    [ObservableProperty]
    private TimeSpan _endTime = new TimeSpan(10, 0, 0);

    [ObservableProperty]
    private bool _isAllDay;

    [ObservableProperty]
    private ObservableCollection<CalendarItemViewModel> _calendars = new();

    [ObservableProperty]
    private CalendarItemViewModel? _selectedCalendar;

    [ObservableProperty]
    private int? _editingEventId;

    public EventEditorViewModel(
        ICalendarRepository calendarRepository,
        IAccountRepository accountRepository,
        ILogger logger)
    {
        _calendarRepository = calendarRepository;
        _accountRepository = accountRepository;
        _logger = logger;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        try
        {
            var account = await _accountRepository.GetDefaultAccountAsync();
            if (account == null) return;

            var calendars = await _calendarRepository.GetCalendarsAsync(account.Id);
            Calendars.Clear();
            foreach (var calendar in calendars)
            {
                Calendars.Add(new CalendarItemViewModel(calendar));
            }

            if (Calendars.Any())
            {
                SelectedCalendar = Calendars.First();
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading event editor");
        }
    }

    public async Task LoadEventAsync(int eventId)
    {
        try
        {
            var calendarEvent = await _calendarRepository.GetEventByIdAsync(eventId);
            if (calendarEvent == null) return;

            EditingEventId = eventId;
            Title = calendarEvent.Title;
            Description = calendarEvent.Description;
            Location = calendarEvent.Location;
            IsAllDay = calendarEvent.IsAllDay;

            var localStart = calendarEvent.StartUtc.ToLocalTime();
            var localEnd = calendarEvent.EndUtc.ToLocalTime();

            StartDate = localStart.Date;
            StartTime = localStart.TimeOfDay;
            EndDate = localEnd.Date;
            EndTime = localEnd.TimeOfDay;

            await LoadAsync();

            var calendar = Calendars.FirstOrDefault(c => c.Id == calendarEvent.CalendarId);
            if (calendar != null)
            {
                SelectedCalendar = calendar;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading event");
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            if (SelectedCalendar == null)
            {
                _logger.Warning("No calendar selected");
                return;
            }

            var calendarEvent = new CalendarEvent
            {
                Uid = Guid.NewGuid().ToString(),
                Title = Title,
                Description = Description,
                Location = Location,
                IsAllDay = IsAllDay,
                CalendarId = SelectedCalendar.Id
            };

            if (IsAllDay)
            {
                calendarEvent.StartUtc = StartDate.Date.ToUniversalTime();
                calendarEvent.EndUtc = EndDate.Date.AddDays(1).ToUniversalTime();
            }
            else
            {
                calendarEvent.StartUtc = StartDate.Date.Add(StartTime).ToUniversalTime();
                calendarEvent.EndUtc = EndDate.Date.Add(EndTime).ToUniversalTime();
            }

            if (EditingEventId.HasValue)
            {
                calendarEvent.Id = EditingEventId.Value;
                await _calendarRepository.UpdateEventAsync(calendarEvent);
                _logger.Information("Updated event {Title}", Title);
            }
            else
            {
                await _calendarRepository.CreateEventAsync(calendarEvent);
                _logger.Information("Created event {Title}", Title);
            }

            RequestClose?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error saving event");
        }
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        try
        {
            if (!EditingEventId.HasValue) return;

            await _calendarRepository.DeleteEventAsync(EditingEventId.Value);
            _logger.Information("Deleted event");

            RequestClose?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error deleting event");
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        RequestClose?.Invoke(this, false);
    }

    public event EventHandler<bool>? RequestClose;

    public bool SavedSuccessfully { get; private set; }
}
