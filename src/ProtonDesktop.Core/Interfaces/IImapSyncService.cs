using ProtonDesktop.Core.Enums;
using ProtonDesktop.Core.Models;

namespace ProtonDesktop.Core.Interfaces;

public interface IImapSyncService
{
    Task<bool> ConnectAsync(MailAccount account);
    Task DisconnectAsync();
    Task<IEnumerable<EmailFolder>> SyncFoldersAsync(MailAccount account);
    Task<IEnumerable<EmailMessage>> SyncMessagesAsync(EmailFolder folder);
    Task<IEnumerable<EmailMessage>> SyncNewMessagesAsync(EmailFolder folder, string? lastUid);
    Task<EmailMessage?> FetchMessageAsync(EmailFolder folder, string uid);
    Task<byte[]?> FetchAttachmentAsync(EmailAttachment attachment, MailAccount account);
    Task<IEnumerable<EmailAttachment>> DownloadAttachmentsAsync(EmailMessage message, EmailFolder folder);
    Task UpdateFlagsAsync(EmailFolder folder, string uid, EmailFlag flags);
    bool IsConnected { get; }
    string? LastError { get; }
}
