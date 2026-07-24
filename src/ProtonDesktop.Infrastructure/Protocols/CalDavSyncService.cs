using System.Net;
using System.Text;
using System.Xml.Linq;
using Ical.Net.DataTypes;
using ProtonDesktop.Core.Enums;
using ProtonDesktop.Core.Interfaces;
using ProtonDesktop.Core.Models;
using Serilog;
using IcalCalendar = Ical.Net.Calendar;
using IcalEvent = Ical.Net.CalendarComponents.CalendarEvent;

namespace ProtonDesktop.Infrastructure.Protocols;

public class CalDavSyncService : ICalDavSyncService
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;
    private readonly ICredentialStore _credentialStore;

    public CalDavSyncService(ILogger logger, ICredentialStore credentialStore)
    {
        _logger = logger;
        _credentialStore = credentialStore;
        _httpClient = new HttpClient();
    }

    public async Task<IEnumerable<Calendar>> SyncCalendarsAsync(MailAccount account)
    {
        try
        {
            _logger.Information("Syncing calendars from CalDAV for {Email}", account.Email);

            var password = _credentialStore.Decrypt(account.EncryptedPassword);
            var baseUrl = $"http://{account.CalDavHost}:{account.CalDavPort}";
            
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Authorization", 
                $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{account.Email}:{password}"))}");

            var calendars = new List<Calendar>();

            var propfindXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<D:propfind xmlns:D=""DAV:"" xmlns:C=""urn:ietf:params:xml:ns:caldav"">
    <D:prop>
        <D:displayname />
        <C:calendar-description />
        <C:calendar-color xmlns:A=""http://apple.com/ns/ical/"" />
    </D:prop>
</D:propfind>";

            var content = new StringContent(propfindXml, Encoding.UTF8, "application/xml");
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Parse("PROPFIND"), $"{baseUrl}/principals/{account.Email}/")
            {
                Content = content,
                Headers = { { "Depth", "1" } }
            });

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                var doc = XDocument.Parse(responseBody);
                
                var nsDav = XNamespace.Get("DAV:");
                var nsCal = XNamespace.Get("urn:ietf:params:xml:ns:caldav");
                var nsApple = XNamespace.Get("http://apple.com/ns/ical/");

                var responses = doc.Descendants(nsDav + "response");
                foreach (var resp in responses)
                {
                    var href = resp.Element(nsDav + "href")?.Value;
                    if (string.IsNullOrEmpty(href) || href.EndsWith("/principals/")) continue;

                    var displayName = resp.Descendants(nsDav + "displayname").FirstOrDefault()?.Value ?? "Calendar";
                    var description = resp.Descendants(nsCal + "calendar-description").FirstOrDefault()?.Value;
                    var color = resp.Descendants(nsApple + "calendar-color").FirstOrDefault()?.Value ?? "#0078D4";

                    calendars.Add(new Calendar
                    {
                        Name = displayName,
                        Description = description,
                        Color = color,
                        SyncToken = href
                    });
                }
            }

            if (calendars.Count == 0)
            {
                calendars.Add(new Calendar
                {
                    Name = "Default Calendar",
                    Color = "#0078D4",
                    SyncToken = $"/calendars/{account.Email}/default/"
                });
            }

            _logger.Information("Found {Count} calendars", calendars.Count);
            return calendars;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error syncing calendars");
            return new List<Calendar>
            {
                new() { Name = "Default Calendar", Color = "#0078D4" }
            };
        }
    }

    public async Task<IEnumerable<CalendarEvent>> SyncEventsAsync(Calendar calendar)
    {
        try
        {
            _logger.Information("Syncing events from CalDAV for calendar {Name}", calendar.Name);

            if (string.IsNullOrEmpty(calendar.SyncToken))
                return Enumerable.Empty<CalendarEvent>();

            var reportXml = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<C:calendar-multiget xmlns:D=""DAV:"" xmlns:C=""urn:ietf:params:xml:ns:caldav"">
    <D:prop>
        <D:getetag />
        <C:calendar-data />
    </D:prop>
    <D:href>{0}</D:href>
</C:calendar-multiget>";

            var content = new StringContent(string.Format(reportXml, calendar.SyncToken), Encoding.UTF8, "application/xml");
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Parse("REPORT"), calendar.SyncToken)
            {
                Content = content
            });

            if (!response.IsSuccessStatusCode)
                return Enumerable.Empty<CalendarEvent>();

            var responseBody = await response.Content.ReadAsStringAsync();
            var doc = XDocument.Parse(responseBody);
            
            var nsDav = XNamespace.Get("DAV:");
            var nsCal = XNamespace.Get("urn:ietf:params:xml:ns:caldav");

            var events = new List<CalendarEvent>();
            var responses = doc.Descendants(nsDav + "response");

            foreach (var resp in responses)
            {
                var etag = resp.Descendants(nsDav + "getetag").FirstOrDefault()?.Value;
                var calendarData = resp.Descendants(nsCal + "calendar-data").FirstOrDefault()?.Value;

                if (string.IsNullOrEmpty(calendarData)) continue;

                try
                {
                    var icalCalendar = IcalCalendar.Load(calendarData);
                    if (icalCalendar?.Events == null) continue;

                    foreach (var icalEvent in icalCalendar.Events)
                    {
                        var calendarEvent = MapIcalEventToCalendarEvent(icalEvent);
                        calendarEvent.ETag = etag;
                        events.Add(calendarEvent);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error parsing ICalendar data");
                }
            }

            _logger.Information("Synced {Count} events", events.Count);
            return events;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error syncing events");
            return Enumerable.Empty<CalendarEvent>();
        }
    }

    public async Task<CalendarEvent?> FetchEventAsync(Calendar calendar, string uid)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{calendar.SyncToken}{uid}.ics");
            if (!response.IsSuccessStatusCode) return null;

            var calendarData = await response.Content.ReadAsStringAsync();
            var icalCalendar = IcalCalendar.Load(calendarData);
            var icalEvent = icalCalendar?.Events.FirstOrDefault();
            
            if (icalEvent == null) return null;

            return MapIcalEventToCalendarEvent(icalEvent);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error fetching event {Uid}", uid);
            return null;
        }
    }

    public async Task<CalendarEvent> CreateEventAsync(Calendar calendar, CalendarEvent calendarEvent)
    {
        try
        {
            var icalEvent = MapCalendarEventToIcalEvent(calendarEvent);
            var icalCalendar = new IcalCalendar();
            icalCalendar.Events.Add(icalEvent);

            var serializer = new Ical.Net.Serialization.CalendarSerializer();
            var icalData = serializer.SerializeToString(icalCalendar);

            var uid = Guid.NewGuid().ToString();
            calendarEvent.Uid = uid;

            var content = new StringContent(icalData, Encoding.UTF8, "text/calendar");
            var response = await _httpClient.PutAsync($"{calendar.SyncToken}{uid}.ics", content);

            if (response.IsSuccessStatusCode)
            {
                calendarEvent.ETag = response.Headers.ETag?.Tag;
                _logger.Information("Created event {Title} with UID {Uid}", calendarEvent.Title, uid);
            }

            return calendarEvent;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error creating event");
            throw;
        }
    }

    public async Task<CalendarEvent> UpdateEventAsync(Calendar calendar, CalendarEvent calendarEvent)
    {
        try
        {
            var icalEvent = MapCalendarEventToIcalEvent(calendarEvent);
            var icalCalendar = new IcalCalendar();
            icalCalendar.Events.Add(icalEvent);

            var serializer = new Ical.Net.Serialization.CalendarSerializer();
            var icalData = serializer.SerializeToString(icalCalendar);

            var content = new StringContent(icalData, Encoding.UTF8, "text/calendar");
            var request = new HttpRequestMessage(HttpMethod.Put, $"{calendar.SyncToken}{calendarEvent.Uid}.ics")
            {
                Content = content
            };

            if (!string.IsNullOrEmpty(calendarEvent.ETag))
            {
                request.Headers.IfMatch.Add(new System.Net.Http.Headers.EntityTagHeaderValue(calendarEvent.ETag));
            }

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                calendarEvent.ETag = response.Headers.ETag?.Tag;
                _logger.Information("Updated event {Title}", calendarEvent.Title);
            }

            return calendarEvent;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error updating event");
            throw;
        }
    }

    public async Task DeleteEventAsync(Calendar calendar, string uid)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"{calendar.SyncToken}{uid}.ics");
            
            if (response.IsSuccessStatusCode)
            {
                _logger.Information("Deleted event {Uid}", uid);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error deleting event {Uid}", uid);
            throw;
        }
    }

    private static CalendarEvent MapIcalEventToCalendarEvent(IcalEvent icalEvent)
    {
        var calendarEvent = new CalendarEvent
        {
            Uid = icalEvent.Uid,
            Title = icalEvent.Summary ?? string.Empty,
            Description = icalEvent.Description,
            Location = icalEvent.Location,
            StartUtc = icalEvent.DtStart?.AsUtc ?? DateTime.UtcNow,
            EndUtc = icalEvent.DtEnd?.AsUtc ?? DateTime.UtcNow.AddHours(1),
            IsAllDay = icalEvent.IsAllDay
        };

        if (icalEvent.RecurrenceRules?.Any() == true)
        {
            var rrule = icalEvent.RecurrenceRules.First();
            calendarEvent.RecurrenceRule = rrule.ToString();
            calendarEvent.Recurrence = EventRecurrence.None;
        }

        return calendarEvent;
    }

    private static IcalEvent MapCalendarEventToIcalEvent(CalendarEvent calendarEvent)
    {
        var icalEvent = new IcalEvent
        {
            Uid = calendarEvent.Uid,
            Summary = calendarEvent.Title,
            Description = calendarEvent.Description,
            Location = calendarEvent.Location,
            DtStart = new CalDateTime(calendarEvent.StartUtc),
            DtEnd = new CalDateTime(calendarEvent.EndUtc)
        };

        if (!string.IsNullOrEmpty(calendarEvent.RecurrenceRule))
        {
            var rrule = new RecurrencePattern(calendarEvent.RecurrenceRule);
            icalEvent.RecurrenceRules.Add(rrule);
        }

        return icalEvent;
    }
}
