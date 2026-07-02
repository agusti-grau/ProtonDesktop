using ProtonDesktop.Core.Interfaces;
using ProtonDesktop.Core.Models;
using Serilog;

namespace ProtonDesktop.Services.Calendar;

public class CalendarSyncService
{
    private readonly ICalDavSyncService _calDavService;
    private readonly ICalendarRepository _calendarRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger _logger;

    public CalendarSyncService(
        ICalDavSyncService calDavService,
        ICalendarRepository calendarRepository,
        IAccountRepository accountRepository,
        ILogger logger)
    {
        _calDavService = calDavService;
        _calendarRepository = calendarRepository;
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task SyncAllAsync()
    {
        var accounts = await _accountRepository.GetAllAccountsAsync();
        foreach (var account in accounts)
        {
            await SyncAccountAsync(account);
        }
    }

    public async Task SyncAccountAsync(MailAccount account)
    {
        try
        {
            _logger.Information("Syncing calendars for account {Email}", account.Email);

            var calendars = await _calDavService.SyncCalendarsAsync(account);
            foreach (var calendar in calendars)
            {
                var existingCalendar = await _calendarRepository.GetCalendarByIdAsync(calendar.Id);
                if (existingCalendar == null)
                {
                    calendar.MailAccountId = account.Id;
                    await _calendarRepository.CreateCalendarAsync(calendar);
                }
                else
                {
                    existingCalendar.Name = calendar.Name;
                    existingCalendar.Description = calendar.Description;
                    existingCalendar.Color = calendar.Color;
                    existingCalendar.SyncToken = calendar.SyncToken;
                    existingCalendar.LastSyncAt = DateTime.UtcNow;
                    await _calendarRepository.UpdateCalendarAsync(existingCalendar);
                }

                var events = await _calDavService.SyncEventsAsync(calendar);
                foreach (var calendarEvent in events)
                {
                    var existingEvent = await _calendarRepository.GetEventByUidAsync(calendar.Id, calendarEvent.Uid);
                    if (existingEvent == null)
                    {
                        calendarEvent.CalendarId = calendar.Id;
                        await _calendarRepository.CreateEventAsync(calendarEvent);
                    }
                    else
                    {
                        existingEvent.Title = calendarEvent.Title;
                        existingEvent.Description = calendarEvent.Description;
                        existingEvent.Location = calendarEvent.Location;
                        existingEvent.StartUtc = calendarEvent.StartUtc;
                        existingEvent.EndUtc = calendarEvent.EndUtc;
                        existingEvent.IsAllDay = calendarEvent.IsAllDay;
                        existingEvent.ETag = calendarEvent.ETag;
                        existingEvent.UpdatedAt = DateTime.UtcNow;
                        await _calendarRepository.UpdateEventAsync(existingEvent);
                    }
                }
            }

            _logger.Information("Calendar sync completed for account {Email}", account.Email);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error syncing calendars for account {Email}", account.Email);
        }
    }
}
