using ProtonDesktop.Core.Models;

namespace ProtonDesktop.Core.Interfaces;

public interface ISmtpService
{
    Task SendAsync(MailAccount account, EmailMessage message, IEnumerable<EmailAttachment>? attachments = null);
    Task<EmailMessage> SaveDraftAsync(MailAccount account, EmailMessage message, IEnumerable<EmailAttachment>? attachments = null);
    Task SendReplyAsync(MailAccount account, EmailMessage originalMessage, EmailMessage replyMessage, IEnumerable<EmailAttachment>? attachments = null);
    Task SendForwardAsync(MailAccount account, EmailMessage originalMessage, EmailMessage forwardMessage, IEnumerable<EmailAttachment>? attachments = null);
}
