using Microsoft.EntityFrameworkCore;
using ProtonDesktop.Core.Interfaces;
using ProtonDesktop.Core.Models;
using ProtonDesktop.Infrastructure.Data;

namespace ProtonDesktop.Infrastructure.Repositories;

public class CalendarRepository : ICalendarRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public CalendarRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<Calendar?> GetCalendarByIdAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Calendars.FindAsync(id);
    }

    public async Task<IEnumerable<Calendar>> GetCalendarsAsync(int accountId)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.Calendars
            .Where(x => x.MailAccountId == accountId)
            .ToListAsync();
    }

    public async Task<Calendar> CreateCalendarAsync(Calendar calendar)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Calendars.Add(calendar);
        await context.SaveChangesAsync();
        return calendar;
    }

    public async Task UpdateCalendarAsync(Calendar calendar)
    {
        using var context = _contextFactory.CreateDbContext();
        context.Calendars.Update(calendar);
        await context.SaveChangesAsync();
    }

    public async Task DeleteCalendarAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var calendar = await context.Calendars.FindAsync(id);
        if (calendar != null)
        {
            context.Calendars.Remove(calendar);
            await context.SaveChangesAsync();
        }
    }

    public async Task<CalendarEvent?> GetEventByIdAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.CalendarEvents
            .Include(x => x.Reminders)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<CalendarEvent?> GetEventByUidAsync(int calendarId, string uid)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.CalendarEvents
            .Include(x => x.Reminders)
            .FirstOrDefaultAsync(x => x.CalendarId == calendarId && x.Uid == uid);
    }

    public async Task<IEnumerable<CalendarEvent>> GetEventsAsync(int calendarId, DateTime start, DateTime end)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.CalendarEvents
            .Where(x => x.CalendarId == calendarId && x.DeletedAt == null)
            .Where(x => x.StartUtc <= end && x.EndUtc >= start)
            .OrderBy(x => x.StartUtc)
            .ToListAsync();
    }

    public async Task<IEnumerable<CalendarEvent>> GetAllEventsAsync(int accountId, DateTime start, DateTime end)
    {
        using var context = _contextFactory.CreateDbContext();
        var calendarIds = await context.Calendars
            .Where(x => x.MailAccountId == accountId)
            .Select(x => x.Id)
            .ToListAsync();

        return await context.CalendarEvents
            .Where(x => calendarIds.Contains(x.CalendarId) && x.DeletedAt == null)
            .Where(x => x.StartUtc <= end && x.EndUtc >= start)
            .OrderBy(x => x.StartUtc)
            .ToListAsync();
    }

    public async Task<CalendarEvent> CreateEventAsync(CalendarEvent calendarEvent)
    {
        using var context = _contextFactory.CreateDbContext();
        context.CalendarEvents.Add(calendarEvent);
        await context.SaveChangesAsync();
        return calendarEvent;
    }

    public async Task UpdateEventAsync(CalendarEvent calendarEvent)
    {
        using var context = _contextFactory.CreateDbContext();
        context.CalendarEvents.Update(calendarEvent);
        await context.SaveChangesAsync();
    }

    public async Task DeleteEventAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var calendarEvent = await context.CalendarEvents.FindAsync(id);
        if (calendarEvent != null)
        {
            context.CalendarEvents.Remove(calendarEvent);
            await context.SaveChangesAsync();
        }
    }

    public async Task SoftDeleteEventAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var calendarEvent = await context.CalendarEvents.FindAsync(id);
        if (calendarEvent != null)
        {
            calendarEvent.DeletedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }

    public async Task<CalendarReminder?> GetReminderByIdAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.CalendarReminders.FindAsync(id);
    }

    public async Task<IEnumerable<CalendarReminder>> GetPendingRemindersAsync(DateTime before)
    {
        using var context = _contextFactory.CreateDbContext();
        var eventStartQuery = context.CalendarEvents
            .Where(x => x.DeletedAt == null)
            .Select(x => new { x.Id, x.StartUtc });

        var reminders = await context.CalendarReminders
            .Where(x => !x.IsSent)
            .Join(eventStartQuery, r => r.CalendarEventId, e => e.Id, (r, e) => new { Reminder = r, EventStart = e.StartUtc })
            .Where(x => x.EventStart.AddMinutes(-x.Reminder.MinutesBefore) <= before)
            .Select(x => x.Reminder)
            .ToListAsync();

        return reminders;
    }

    public async Task<CalendarReminder> CreateReminderAsync(CalendarReminder reminder)
    {
        using var context = _contextFactory.CreateDbContext();
        context.CalendarReminders.Add(reminder);
        await context.SaveChangesAsync();
        return reminder;
    }

    public async Task UpdateReminderAsync(CalendarReminder reminder)
    {
        using var context = _contextFactory.CreateDbContext();
        context.CalendarReminders.Update(reminder);
        await context.SaveChangesAsync();
    }

    public async Task MarkReminderSentAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var reminder = await context.CalendarReminders.FindAsync(id);
        if (reminder != null)
        {
            reminder.IsSent = true;
            reminder.SentAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }
}
