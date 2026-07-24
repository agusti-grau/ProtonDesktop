using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonDesktop.Core.Interfaces;
using ProtonDesktop.Core.Models;
using Serilog;
using System.Collections.ObjectModel;

namespace ProtonDesktop.ViewModels.Calendar;

public partial class CalendarViewModel : ObservableObject
{
    private readonly ICalendarRepository _calendarRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger _logger;

    [ObservableProperty]
    private DateTime _currentDate = DateTime.Today;

    [ObservableProperty]
    private CalendarViewMode _viewMode = CalendarViewMode.Month;

    [ObservableProperty]
    private ObservableCollection<CalendarItemViewModel> _calendars = new();

    [ObservableProperty]
    private ObservableCollection<CalendarEventViewModel> _events = new();

    [ObservableProperty]
    private string _title = "Calendar";

    public CalendarViewModel(
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
            if (account == null)
            {
                _logger.Warning("No default account found");
                return;
            }

            var calendars = await _calendarRepository.GetCalendarsAsync(account.Id);
            Calendars.Clear();
            foreach (var calendar in calendars)
            {
                Calendars.Add(new CalendarItemViewModel(calendar));
            }

            await LoadEventsAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading calendar");
        }
    }

    [RelayCommand]
    private async Task LoadEventsAsync()
    {
        try
        {
            var account = await _accountRepository.GetDefaultAccountAsync();
            if (account == null) return;

            DateTime start, end;
            switch (ViewMode)
            {
                case CalendarViewMode.Day:
                    start = CurrentDate.Date;
                    end = start.AddDays(1);
                    break;
                case CalendarViewMode.Week:
                    start = CurrentDate.Date.AddDays(-(int)CurrentDate.DayOfWeek);
                    end = start.AddDays(7);
                    break;
                case CalendarViewMode.Month:
                default:
                    start = new DateTime(CurrentDate.Year, CurrentDate.Month, 1);
                    end = start.AddMonths(1);
                    break;
            }

            var events = await _calendarRepository.GetAllEventsAsync(account.Id, start, end);
            Events.Clear();
            foreach (var evt in events)
            {
                Events.Add(new CalendarEventViewModel(evt));
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading events");
        }
    }

    [RelayCommand]
    private async Task PreviousAsync()
    {
        switch (ViewMode)
        {
            case CalendarViewMode.Day:
                CurrentDate = CurrentDate.AddDays(-1);
                break;
            case CalendarViewMode.Week:
                CurrentDate = CurrentDate.AddDays(-7);
                break;
            case CalendarViewMode.Month:
                CurrentDate = CurrentDate.AddMonths(-1);
                break;
        }
        await LoadEventsAsync();
    }

    [RelayCommand]
    private async Task NextAsync()
    {
        switch (ViewMode)
        {
            case CalendarViewMode.Day:
                CurrentDate = CurrentDate.AddDays(1);
                break;
            case CalendarViewMode.Week:
                CurrentDate = CurrentDate.AddDays(7);
                break;
            case CalendarViewMode.Month:
                CurrentDate = CurrentDate.AddMonths(1);
                break;
        }
        await LoadEventsAsync();
    }

    [RelayCommand]
    private async Task TodayAsync()
    {
        CurrentDate = DateTime.Today;
        await LoadEventsAsync();
    }

    [RelayCommand]
    private void ChangeViewMode(CalendarViewMode mode)
    {
        ViewMode = mode;
        _ = LoadEventsAsync();
    }

    public string GetCurrentTitle()
    {
        return ViewMode switch
        {
            CalendarViewMode.Day => CurrentDate.ToString("dddd, MMMM d, yyyy"),
            CalendarViewMode.Week => $"Week of {CurrentDate.Date.AddDays(-(int)CurrentDate.DayOfWeek):MMMM d, yyyy}",
            CalendarViewMode.Month => CurrentDate.ToString("MMMM yyyy"),
            _ => "Calendar"
        };
    }
}

public enum CalendarViewMode
{
    Day,
    Week,
    Month
}
