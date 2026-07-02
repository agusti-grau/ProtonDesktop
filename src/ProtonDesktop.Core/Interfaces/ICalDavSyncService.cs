using ProtonDesktop.Core.Models;

namespace ProtonDesktop.Core.Interfaces;

public interface ICalDavSyncService
{
    Task<IEnumerable<Calendar>> SyncCalendarsAsync(MailAccount account);
    Task<IEnumerable<CalendarEvent>> SyncEventsAsync(Calendar calendar);
    Task<CalendarEvent?> FetchEventAsync(Calendar calendar, string uid);
    Task<CalendarEvent> CreateEventAsync(Calendar calendar, CalendarEvent calendarEvent);
    Task<CalendarEvent> UpdateEventAsync(Calendar calendar, CalendarEvent calendarEvent);
    Task DeleteEventAsync(Calendar calendar, string uid);
}
