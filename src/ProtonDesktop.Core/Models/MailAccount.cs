namespace ProtonDesktop.Core.Models;

public class MailAccount
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ImapHost { get; set; } = "localhost";
    public int ImapPort { get; set; } = 1143;
    public string SmtpHost { get; set; } = "localhost";
    public int SmtpPort { get; set; } = 1025;
    public string CalDavHost { get; set; } = "localhost";
    public int CalDavPort { get; set; } = 8080;
    public string EncryptedPassword { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastSyncAt { get; set; }

    public ICollection<EmailFolder> Folders { get; set; } = new List<EmailFolder>();
    public ICollection<Calendar> Calendars { get; set; } = new List<Calendar>();
}
