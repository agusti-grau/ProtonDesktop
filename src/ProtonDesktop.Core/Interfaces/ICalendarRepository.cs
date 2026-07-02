using ProtonDesktop.Core.Models;

namespace ProtonDesktop.Core.Interfaces;

public interface ICalendarRepository
{
    Task<Calendar?> GetCalendarByIdAsync(int id);
    Task<IEnumerable<Calendar>> GetCalendarsAsync(int accountId);
    Task<Calendar> CreateCalendarAsync(Calendar calendar);
    Task UpdateCalendarAsync(Calendar calendar);
    Task DeleteCalendarAsync(int id);

    Task<CalendarEvent?> GetEventByIdAsync(int id);
    Task<CalendarEvent?> GetEventByUidAsync(int calendarId, string uid);
    Task<IEnumerable<CalendarEvent>> GetEventsAsync(int calendarId, DateTime start, DateTime end);
    Task<IEnumerable<CalendarEvent>> GetAllEventsAsync(int accountId, DateTime start, DateTime end);
    Task<CalendarEvent> CreateEventAsync(CalendarEvent calendarEvent);
    Task UpdateEventAsync(CalendarEvent calendarEvent);
    Task DeleteEventAsync(int id);
    Task SoftDeleteEventAsync(int id);

    Task<CalendarReminder?> GetReminderByIdAsync(int id);
    Task<IEnumerable<CalendarReminder>> GetPendingRemindersAsync(DateTime before);
    Task<CalendarReminder> CreateReminderAsync(CalendarReminder reminder);
    Task UpdateReminderAsync(CalendarReminder reminder);
    Task MarkReminderSentAsync(int id);
}
