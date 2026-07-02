using ProtonDesktop.Core.Models;

namespace ProtonDesktop.Core.Interfaces;

public interface IImapSyncService
{
    Task<bool> ConnectAsync(MailAccount account);
    Task DisconnectAsync();
    Task<IEnumerable<EmailFolder>> SyncFoldersAsync(MailAccount account);
    Task<IEnumerable<EmailMessage>> SyncMessagesAsync(EmailFolder folder);
    Task<EmailMessage?> FetchMessageAsync(EmailFolder folder, string uid);
    Task<byte[]?> FetchAttachmentAsync(EmailAttachment attachment, MailAccount account);
    bool IsConnected { get; }
}
