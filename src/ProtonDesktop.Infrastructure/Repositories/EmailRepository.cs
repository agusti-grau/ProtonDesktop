using Microsoft.EntityFrameworkCore;
using ProtonDesktop.Core.Interfaces;
using ProtonDesktop.Core.Models;
using ProtonDesktop.Infrastructure.Data;

namespace ProtonDesktop.Infrastructure.Repositories;

public class EmailRepository : IEmailRepository
{
    private readonly AppDbContext _context;

    public EmailRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<EmailFolder?> GetFolderByIdAsync(int id)
    {
        return await _context.EmailFolders
            .Include(x => x.SubFolders)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<EmailFolder?> GetFolderByPathAsync(int accountId, string path)
    {
        return await _context.EmailFolders
            .FirstOrDefaultAsync(x => x.MailAccountId == accountId && x.Path == path);
    }

    public async Task<IEnumerable<EmailFolder>> GetFoldersAsync(int accountId)
    {
        return await _context.EmailFolders
            .Where(x => x.MailAccountId == accountId)
            .Include(x => x.SubFolders)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<EmailFolder> CreateFolderAsync(EmailFolder folder)
    {
        _context.EmailFolders.Add(folder);
        await _context.SaveChangesAsync();
        return folder;
    }

    public async Task UpdateFolderAsync(EmailFolder folder)
    {
        _context.EmailFolders.Update(folder);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteFolderAsync(int id)
    {
        var folder = await _context.EmailFolders.FindAsync(id);
        if (folder != null)
        {
            _context.EmailFolders.Remove(folder);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<EmailMessage?> GetMessageByIdAsync(int id)
    {
        return await _context.EmailMessages
            .Include(x => x.Attachments)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<EmailMessage?> GetMessageByUidAsync(int folderId, string uid)
    {
        return await _context.EmailMessages
            .Include(x => x.Attachments)
            .FirstOrDefaultAsync(x => x.FolderId == folderId && x.Uid == uid);
    }

    public async Task<IEnumerable<EmailMessage>> GetMessagesAsync(int folderId, int skip = 0, int take = 50)
    {
        return await _context.EmailMessages
            .Where(x => x.FolderId == folderId && x.DeletedAt == null)
            .OrderByDescending(x => x.ReceivedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<EmailMessage>> SearchMessagesAsync(int accountId, string query, int skip = 0, int take = 50)
    {
        var folderIds = await _context.EmailFolders
            .Where(x => x.MailAccountId == accountId)
            .Select(x => x.Id)
            .ToListAsync();

        return await _context.EmailMessages
            .Where(x => folderIds.Contains(x.FolderId) && x.DeletedAt == null)
            .Where(x => x.Subject.Contains(query) || x.FromAddress.Contains(query) || x.FromName.Contains(query) || (x.PlainTextBody != null && x.PlainTextBody.Contains(query)))
            .OrderByDescending(x => x.ReceivedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<EmailMessage> CreateMessageAsync(EmailMessage message)
    {
        _context.EmailMessages.Add(message);
        await _context.SaveChangesAsync();
        return message;
    }

    public async Task UpdateMessageAsync(EmailMessage message)
    {
        _context.EmailMessages.Update(message);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteMessageAsync(int id)
    {
        var message = await _context.EmailMessages.FindAsync(id);
        if (message != null)
        {
            _context.EmailMessages.Remove(message);
            await _context.SaveChangesAsync();
        }
    }

    public async Task SoftDeleteMessageAsync(int id)
    {
        var message = await _context.EmailMessages.FindAsync(id);
        if (message != null)
        {
            message.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<EmailAttachment?> GetAttachmentByIdAsync(int id)
    {
        return await _context.EmailAttachments.FindAsync(id);
    }

    public async Task<EmailAttachment> CreateAttachmentAsync(EmailAttachment attachment)
    {
        _context.EmailAttachments.Add(attachment);
        await _context.SaveChangesAsync();
        return attachment;
    }

    public async Task DeleteAttachmentAsync(int id)
    {
        var attachment = await _context.EmailAttachments.FindAsync(id);
        if (attachment != null)
        {
            _context.EmailAttachments.Remove(attachment);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int> GetUnreadCountAsync(int folderId)
    {
        return await _context.EmailMessages
            .Where(x => x.FolderId == folderId && x.DeletedAt == null && !x.Flags.HasFlag(Core.Enums.EmailFlag.Seen))
            .CountAsync();
    }

    public async Task UpdateUnreadCountsAsync(int accountId)
    {
        var folders = await _context.EmailFolders
            .Where(x => x.MailAccountId == accountId)
            .ToListAsync();

        foreach (var folder in folders)
        {
            folder.UnreadCount = await _context.EmailMessages
                .Where(x => x.FolderId == folder.Id && x.DeletedAt == null && !x.Flags.HasFlag(Core.Enums.EmailFlag.Seen))
                .CountAsync();

            folder.TotalCount = await _context.EmailMessages
                .Where(x => x.FolderId == folder.Id && x.DeletedAt == null)
                .CountAsync();
        }

        await _context.SaveChangesAsync();
    }
}
