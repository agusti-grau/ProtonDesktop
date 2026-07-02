using ProtonDesktop.Core.Enums;

namespace ProtonDesktop.Core.Models;

public class EmailMessage
{
    public int Id { get; set; }
    public string MessageId { get; set; } = string.Empty;
    public string? InReplyTo { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string ToAddresses { get; set; } = string.Empty;
    public string? CcAddresses { get; set; }
    public string? BccAddresses { get; set; }
    public DateTime ReceivedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public string? PlainTextBody { get; set; }
    public string? HtmlBody { get; set; }
    public EmailFlag Flags { get; set; } = EmailFlag.None;
    public bool HasAttachments { get; set; }
    public long? Size { get; set; }
    public string? Uid { get; set; }
    public int FolderId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public EmailFolder Folder { get; set; } = null!;
    public ICollection<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();
}
