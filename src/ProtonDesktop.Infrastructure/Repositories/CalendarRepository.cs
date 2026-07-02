using Microsoft.EntityFrameworkCore;
using ProtonDesktop.Core.Interfaces;
using ProtonDesktop.Core.Models;
using ProtonDesktop.Infrastructure.Data;

namespace ProtonDesktop.Infrastructure.Repositories;

public class CalendarRepository : ICalendarRepository
{
    private readonly AppDbContext _context;

    public CalendarRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Calendar?> GetCalendarByIdAsync(int id)
    {
        return await _context.Calendars.FindAsync(id);
    }

    public async Task<IEnumerable<Calendar>> GetCalendarsAsync(int accountId)
    {
        return await _context.Calendars
            .Where(x => x.MailAccountId == accountId)
            .ToListAsync();
    }

    public async Task<Calendar> CreateCalendarAsync(Calendar calendar)
    {
        _context.Calendars.Add(calendar);
        await _context.SaveChangesAsync();
        return calendar;
    }

    public async Task UpdateCalendarAsync(Calendar calendar)
    {
        _context.Calendars.Update(calendar);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCalendarAsync(int id)
    {
        var calendar = await _context.Calendars.FindAsync(id);
        if (calendar != null)
        {
            _context.Calendars.Remove(calendar);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<CalendarEvent?> GetEventByIdAsync(int id)
    {
        return await _context.CalendarEvents
            .Include(x => x.Reminders)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<CalendarEvent?> GetEventByUidAsync(int calendarId, string uid)
    {
        return await _context.CalendarEvents
            .Include(x => x.Reminders)
            .FirstOrDefaultAsync(x => x.CalendarId == calendarId && x.Uid == uid);
    }

    public async Task<IEnumerable<CalendarEvent>> GetEventsAsync(int calendarId, DateTime start, DateTime end)
    {
        return await _context.CalendarEvents
            .Where(x => x.CalendarId == calendarId && x.DeletedAt == null)
            .Where(x => x.StartUtc <= end && x.EndUtc >= start)
            .OrderBy(x => x.StartUtc)
            .ToListAsync();
    }

    public async Task<IEnumerable<CalendarEvent>> GetAllEventsAsync(int accountId, DateTime start, DateTime end)
    {
        var calendarIds = await _context.Calendars
            .Where(x => x.MailAccountId == accountId)
            .Select(x => x.Id)
            .ToListAsync();

        return await _context.CalendarEvents
            .Where(x => calendarIds.Contains(x.CalendarId) && x.DeletedAt == null)
            .Where(x => x.StartUtc <= end && x.EndUtc >= start)
            .OrderBy(x => x.StartUtc)
            .ToListAsync();
    }

    public async Task<CalendarEvent> CreateEventAsync(CalendarEvent calendarEvent)
    {
        _context.CalendarEvents.Add(calendarEvent);
        await _context.SaveChangesAsync();
        return calendarEvent;
    }

    public async Task UpdateEventAsync(CalendarEvent calendarEvent)
    {
        _context.CalendarEvents.Update(calendarEvent);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteEventAsync(int id)
    {
        var calendarEvent = await _context.CalendarEvents.FindAsync(id);
        if (calendarEvent != null)
        {
            _context.CalendarEvents.Remove(calendarEvent);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SoftDeleteEventAsync(int id)
    {
        var calendarEvent = await _context.CalendarEvents.FindAsync(id);
        if (calendarEvent != null)
        {
            calendarEvent.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<CalendarReminder?> GetReminderByIdAsync(int id)
    {
        return await _context.CalendarReminders.FindAsync(id);
    }

    public async Task<IEnumerable<CalendarReminder>> GetPendingRemindersAsync(DateTime before)
    {
        var eventStartQuery = _context.CalendarEvents
            .Where(x => x.DeletedAt == null)
            .Select(x => new { x.Id, x.StartUtc });

        var reminders = await _context.CalendarReminders
            .Where(x => !x.IsSent)
            .Join(eventStartQuery, r => r.CalendarEventId, e => e.Id, (r, e) => new { Reminder = r, EventStart = e.StartUtc })
            .Where(x => x.EventStart.AddMinutes(-x.Reminder.MinutesBefore) <= before)
            .Select(x => x.Reminder)
            .ToListAsync();

        return reminders;
    }

    public async Task<CalendarReminder> CreateReminderAsync(CalendarReminder reminder)
    {
        _context.CalendarReminders.Add(reminder);
        await _context.SaveChangesAsync();
        return reminder;
    }

    public async Task UpdateReminderAsync(CalendarReminder reminder)
    {
        _context.CalendarReminders.Update(reminder);
        await _context.SaveChangesAsync();
    }

    public async Task MarkReminderSentAsync(int id)
    {
        var reminder = await _context.CalendarReminders.FindAsync(id);
        if (reminder != null)
        {
            reminder.IsSent = true;
            reminder.SentAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
