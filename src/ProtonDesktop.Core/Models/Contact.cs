namespace ProtonDesktop.Core.Models;

public class Contact
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Company { get; set; }
    public string? Notes { get; set; }
    public int MailAccountId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public MailAccount MailAccount { get; set; } = null!;
}
