using ProtonDesktop.Core.Enums;

namespace ProtonDesktop.Core.Models;

public class EmailFolder
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public FolderType FolderType { get; set; }
    public int? ParentFolderId { get; set; }
    public int MailAccountId { get; set; }
    public string? UidNext { get; set; }
    public string? UidValidity { get; set; }
    public int UnreadCount { get; set; }
    public int TotalCount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastSyncAt { get; set; }

    public EmailFolder? ParentFolder { get; set; }
    public MailAccount MailAccount { get; set; } = null!;
    public ICollection<EmailFolder> SubFolders { get; set; } = new List<EmailFolder>();
    public ICollection<EmailMessage> Messages { get; set; } = new List<EmailMessage>();
}
