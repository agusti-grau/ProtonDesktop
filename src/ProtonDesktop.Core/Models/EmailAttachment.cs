namespace ProtonDesktop.Core.Models;

public class EmailAttachment
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string? ContentId { get; set; }
    public bool IsInline { get; set; }
    public string LocalPath { get; set; } = string.Empty;
    public int EmailMessageId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public EmailMessage EmailMessage { get; set; } = null!;
}
