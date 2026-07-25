using Microsoft.EntityFrameworkCore;
using ProtonDesktop.Core.Enums;
using ProtonDesktop.Core.Interfaces;
using ProtonDesktop.Core.Models;
using ProtonDesktop.Infrastructure.Data;

namespace ProtonDesktop.Infrastructure.Repositories;

public class EmailRepository : IEmailRepository
{
    private readonly IDbContextFactory<AppDbContext> _contextFactory;

    public EmailRepository(IDbContextFactory<AppDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<EmailFolder?> GetFolderByIdAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.EmailFolders
            .Include(x => x.SubFolders)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<EmailFolder?> GetFolderByPathAsync(int accountId, string path)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.EmailFolders
            .FirstOrDefaultAsync(x => x.MailAccountId == accountId && x.Path == path);
    }

    public async Task<IEnumerable<EmailFolder>> GetFoldersAsync(int accountId)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.EmailFolders
            .Where(x => x.MailAccountId == accountId)
            .Include(x => x.SubFolders)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<EmailFolder> CreateFolderAsync(EmailFolder folder)
    {
        using var context = _contextFactory.CreateDbContext();
        context.EmailFolders.Add(folder);
        await context.SaveChangesAsync();
        return folder;
    }

    public async Task UpdateFolderAsync(EmailFolder folder)
    {
        using var context = _contextFactory.CreateDbContext();
        context.EmailFolders.Update(folder);
        await context.SaveChangesAsync();
    }

    public async Task DeleteFolderAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var folder = await context.EmailFolders.FindAsync(id);
        if (folder != null)
        {
            context.EmailFolders.Remove(folder);
            await context.SaveChangesAsync();
        }
    }

    public async Task<EmailMessage?> GetMessageByIdAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.EmailMessages
            .Include(x => x.Attachments)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<EmailMessage?> GetMessageByUidAsync(int folderId, string uid)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.EmailMessages
            .Include(x => x.Attachments)
            .FirstOrDefaultAsync(x => x.FolderId == folderId && x.Uid == uid);
    }

    public async Task<IEnumerable<EmailMessage>> GetMessagesAsync(int folderId, int skip = 0, int take = 50)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.EmailMessages
            .Where(x => x.FolderId == folderId && x.DeletedAt == null)
            .OrderByDescending(x => x.ReceivedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<IEnumerable<EmailMessage>> SearchMessagesAsync(int accountId, string query, int skip = 0, int take = 50)
    {
        using var context = _contextFactory.CreateDbContext();
        var folderIds = await context.EmailFolders
            .Where(x => x.MailAccountId == accountId)
            .Select(x => x.Id)
            .ToListAsync();

        return await context.EmailMessages
            .Where(x => folderIds.Contains(x.FolderId) && x.DeletedAt == null)
            .Where(x => x.Subject.Contains(query) || x.FromAddress.Contains(query) || x.FromName.Contains(query) || (x.PlainTextBody != null && x.PlainTextBody.Contains(query)))
            .OrderByDescending(x => x.ReceivedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<EmailMessage> CreateMessageAsync(EmailMessage message)
    {
        using var context = _contextFactory.CreateDbContext();
        context.EmailMessages.Add(message);
        await context.SaveChangesAsync();
        return message;
    }

    public async Task UpdateMessageAsync(EmailMessage message)
    {
        using var context = _contextFactory.CreateDbContext();
        context.EmailMessages.Update(message);
        await context.SaveChangesAsync();
    }

    public async Task DeleteMessageAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var message = await context.EmailMessages.FindAsync(id);
        if (message != null)
        {
            context.EmailMessages.Remove(message);
            await context.SaveChangesAsync();
        }
    }

    public async Task SoftDeleteMessageAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var message = await context.EmailMessages.FindAsync(id);
        if (message != null)
        {
            message.DeletedAt = DateTime.UtcNow;
            await context.SaveChangesAsync();
        }
    }

    public async Task<EmailAttachment?> GetAttachmentByIdAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.EmailAttachments.FindAsync(id);
    }

    public async Task<EmailAttachment> CreateAttachmentAsync(EmailAttachment attachment)
    {
        using var context = _contextFactory.CreateDbContext();
        context.EmailAttachments.Add(attachment);
        await context.SaveChangesAsync();
        return attachment;
    }

    public async Task DeleteAttachmentAsync(int id)
    {
        using var context = _contextFactory.CreateDbContext();
        var attachment = await context.EmailAttachments.FindAsync(id);
        if (attachment != null)
        {
            context.EmailAttachments.Remove(attachment);
            await context.SaveChangesAsync();
        }
    }

    public async Task<int> GetUnreadCountAsync(int folderId)
    {
        using var context = _contextFactory.CreateDbContext();
        return await context.EmailMessages
            .Where(x => x.FolderId == folderId && x.DeletedAt == null && !x.Flags.HasFlag(EmailFlag.Seen))
            .CountAsync();
    }

    public async Task UpdateUnreadCountsAsync(int accountId)
    {
        using var context = _contextFactory.CreateDbContext();
        var folders = await context.EmailFolders
            .Where(x => x.MailAccountId == accountId)
            .ToListAsync();

        foreach (var folder in folders)
        {
            folder.UnreadCount = await context.EmailMessages
                .Where(x => x.FolderId == folder.Id && x.DeletedAt == null && !x.Flags.HasFlag(EmailFlag.Seen))
                .CountAsync();

            folder.TotalCount = await context.EmailMessages
                .Where(x => x.FolderId == folder.Id && x.DeletedAt == null)
                .CountAsync();
        }

        await context.SaveChangesAsync();
    }
}
