using ProtonDesktop.Core.Models;

namespace ProtonDesktop.Core.Interfaces;

public interface IEmailRepository
{
    Task<EmailFolder?> GetFolderByIdAsync(int id);
    Task<EmailFolder?> GetFolderByPathAsync(int accountId, string path);
    Task<IEnumerable<EmailFolder>> GetFoldersAsync(int accountId);
    Task<EmailFolder> CreateFolderAsync(EmailFolder folder);
    Task UpdateFolderAsync(EmailFolder folder);
    Task DeleteFolderAsync(int id);

    Task<EmailMessage?> GetMessageByIdAsync(int id);
    Task<EmailMessage?> GetMessageByUidAsync(int folderId, string uid);
    Task<IEnumerable<EmailMessage>> GetMessagesAsync(int folderId, int skip = 0, int take = 50);
    Task<IEnumerable<EmailMessage>> SearchMessagesAsync(int accountId, string query, int skip = 0, int take = 50);
    Task<EmailMessage> CreateMessageAsync(EmailMessage message);
    Task UpdateMessageAsync(EmailMessage message);
    Task DeleteMessageAsync(int id);
    Task SoftDeleteMessageAsync(int id);

    Task<EmailAttachment?> GetAttachmentByIdAsync(int id);
    Task<EmailAttachment> CreateAttachmentAsync(EmailAttachment attachment);
    Task DeleteAttachmentAsync(int id);

    Task<int> GetUnreadCountAsync(int folderId);
    Task UpdateUnreadCountsAsync(int accountId);
}
