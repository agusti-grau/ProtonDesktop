using ProtonDesktop.Core.Enums;

namespace ProtonDesktop.Core.Models;

public class CalendarReminder
{
    public int Id { get; set; }
    public ReminderType ReminderType { get; set; } = ReminderType.Popup;
    public int MinutesBefore { get; set; }
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
    public int CalendarEventId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public CalendarEvent CalendarEvent { get; set; } = null!;
}
