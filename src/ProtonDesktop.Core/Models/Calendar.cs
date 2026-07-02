namespace ProtonDesktop.Core.Models;

public class Calendar
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public bool IsVisible { get; set; } = true;
    public bool IsDefault { get; set; }
    public int MailAccountId { get; set; }
    public string? SyncToken { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastSyncAt { get; set; }

    public MailAccount MailAccount { get; set; } = null!;
    public ICollection<CalendarEvent> Events { get; set; } = new List<CalendarEvent>();
}
