using ProtonDesktop.Core.Enums;

namespace ProtonDesktop.Core.Models;

public class CalendarEvent
{
    public int Id { get; set; }
    public string Uid { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Location { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public bool IsAllDay { get; set; }
    public EventRecurrence Recurrence { get; set; } = EventRecurrence.None;
    public string? RecurrenceRule { get; set; }
    public int? RecurrenceParentId { get; set; }
    public DateTime? RecurrenceExceptionDate { get; set; }
    public int CalendarId { get; set; }
    public string? ETag { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }

    public Calendar Calendar { get; set; } = null!;
    public CalendarEvent? RecurrenceParent { get; set; }
    public ICollection<CalendarReminder> Reminders { get; set; } = new List<CalendarReminder>();
}
