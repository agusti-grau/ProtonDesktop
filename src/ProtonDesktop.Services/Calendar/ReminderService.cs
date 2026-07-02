using ProtonDesktop.Core.Interfaces;
using Serilog;

namespace ProtonDesktop.Services.Calendar;

public class ReminderService : IReminderService
{
    private readonly ICalendarRepository _calendarRepository;
    private readonly ILogger _logger;
    private Timer? _timer;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public event EventHandler<ReminderEventArgs>? ReminderTriggered;

    public ReminderService(
        ICalendarRepository calendarRepository,
        ILogger logger)
    {
        _calendarRepository = calendarRepository;
        _logger = logger;
    }

    public Task StartAsync()
    {
        _logger.Information("Starting reminder service");
        _timer = new Timer(CheckReminders, null, TimeSpan.Zero, _checkInterval);
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        _logger.Information("Stopping reminder service");
        _timer?.Dispose();
        _timer = null;
        return Task.CompletedTask;
    }

    private async void CheckReminders(object? state)
    {
        try
        {
            var now = DateTime.UtcNow;
            var reminders = await _calendarRepository.GetPendingRemindersAsync(now);

            foreach (var reminder in reminders)
            {
                var calendarEvent = await _calendarRepository.GetEventByIdAsync(reminder.CalendarEventId);
                if (calendarEvent != null)
                {
                    _logger.Information("Triggering reminder for event {Title}", calendarEvent.Title);
                    ReminderTriggered?.Invoke(this, new ReminderEventArgs
                    {
                        EventId = calendarEvent.Id,
                        EventTitle = calendarEvent.Title,
                        EventStart = calendarEvent.StartUtc,
                        ReminderId = reminder.Id
                    });

                    await _calendarRepository.MarkReminderSentAsync(reminder.Id);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error checking reminders");
        }
    }
}
