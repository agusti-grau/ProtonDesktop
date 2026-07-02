using ProtonDesktop.Core.Interfaces;
using ProtonDesktop.Core.Models;
using Serilog;

namespace ProtonDesktop.Infrastructure.Protocols;

public class CalDavSyncService : ICalDavSyncService
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;

    public CalDavSyncService(ILogger logger)
    {
        _logger = logger;
        _httpClient = new HttpClient();
    }

    public Task<IEnumerable<Calendar>> SyncCalendarsAsync(MailAccount account)
    {
        _logger.Information("Syncing calendars from CalDAV for {Email}", account.Email);
        var calendars = new List<Calendar>
        {
            new() { Name = "Default Calendar", Color = "#0078D4" }
        };
        return Task.FromResult<IEnumerable<Calendar>>(calendars);
    }

    public Task<IEnumerable<CalendarEvent>> SyncEventsAsync(Calendar calendar)
    {
        _logger.Information("Syncing events from CalDAV for calendar {Name}", calendar.Name);
        return Task.FromResult<IEnumerable<CalendarEvent>>(Enumerable.Empty<CalendarEvent>());
    }

    public Task<CalendarEvent?> FetchEventAsync(Calendar calendar, string uid)
    {
        return Task.FromResult<CalendarEvent?>(null);
    }

    public Task<CalendarEvent> CreateEventAsync(Calendar calendar, CalendarEvent calendarEvent)
    {
        _logger.Information("Creating event {Title} in calendar {Name}", calendarEvent.Title, calendar.Name);
        return Task.FromResult(calendarEvent);
    }

    public Task<CalendarEvent> UpdateEventAsync(Calendar calendar, CalendarEvent calendarEvent)
    {
        _logger.Information("Updating event {Title} in calendar {Name}", calendarEvent.Title, calendar.Name);
        return Task.FromResult(calendarEvent);
    }

    public Task DeleteEventAsync(Calendar calendar, string uid)
    {
        _logger.Information("Deleting event {Uid} from calendar {Name}", uid, calendar.Name);
        return Task.CompletedTask;
    }
}
