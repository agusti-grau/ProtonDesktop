using ProtonDesktop.Core.Models;

namespace ProtonDesktop.Core.Interfaces;

public interface ISmtpService
{
    Task SendAsync(MailAccount account, EmailMessage message, IEnumerable<EmailAttachment>? attachments = null);
    Task<EmailMessage> SaveDraftAsync(MailAccount account, EmailMessage message, IEnumerable<EmailAttachment>? attachments = null);
}
