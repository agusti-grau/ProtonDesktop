using ProtonDesktop.Core.Interfaces;
using ProtonDesktop.Core.Models;
using Serilog;

namespace ProtonDesktop.Services.Email;

public class EmailSyncService
{
    private readonly IImapSyncService _imapService;
    private readonly IEmailRepository _emailRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ILogger _logger;

    public EmailSyncService(
        IImapSyncService imapService,
        IEmailRepository emailRepository,
        IAccountRepository accountRepository,
        ILogger logger)
    {
        _imapService = imapService;
        _emailRepository = emailRepository;
        _accountRepository = accountRepository;
        _logger = logger;
    }

    public async Task SyncAllAsync()
    {
        var accounts = await _accountRepository.GetAllAccountsAsync();
        foreach (var account in accounts)
        {
            await SyncAccountAsync(account);
        }
    }

    public async Task SyncAccountAsync(MailAccount account)
    {
        try
        {
            _logger.Information("Syncing account {Email}", account.Email);

            if (!await _imapService.ConnectAsync(account))
            {
                _logger.Error("Failed to connect to IMAP for account {Email}", account.Email);
                return;
            }

            try
            {
                var folders = await _imapService.SyncFoldersAsync(account);
                foreach (var folder in folders)
                {
                    var existingFolder = await _emailRepository.GetFolderByPathAsync(account.Id, folder.Path);

                    EmailFolder targetFolder;
                    string? lastUid;

                    if (existingFolder == null)
                    {
                        // New folder: create it, then fetch ALL messages (lastUid = null)
                        folder.MailAccountId = account.Id;
                        targetFolder = await _emailRepository.CreateFolderAsync(folder);
                        lastUid = null;
                    }
                    else
                    {
                        // Existing folder: fetch only messages newer than stored UidNext
                        targetFolder = existingFolder;
                        lastUid = existingFolder.UidNext;
                    }

                    var newMessages = await _imapService.SyncNewMessagesAsync(targetFolder, lastUid);
                    foreach (var message in newMessages)
                    {
                        var existingMessage = await _emailRepository.GetMessageByUidAsync(targetFolder.Id, message.Uid!);
                        if (existingMessage == null)
                        {
                            message.FolderId = targetFolder.Id;
                            await _emailRepository.CreateMessageAsync(message);

                            if (message.HasAttachments)
                            {
                                var attachments = await _imapService.DownloadAttachmentsAsync(message, targetFolder);
                                foreach (var attachment in attachments)
                                {
                                    attachment.EmailMessageId = message.Id;
                                    await _emailRepository.CreateAttachmentAsync(attachment);
                                }
                            }
                        }
                    }

                    targetFolder.UidNext = folder.UidNext;
                    targetFolder.UidValidity = folder.UidValidity;
                    targetFolder.LastSyncAt = DateTime.UtcNow;
                    await _emailRepository.UpdateFolderAsync(targetFolder);
                }

                await _emailRepository.UpdateUnreadCountsAsync(account.Id);

                account.LastSyncAt = DateTime.UtcNow;
                await _accountRepository.UpdateAccountAsync(account);

                _logger.Information("Sync completed for account {Email}", account.Email);
            }
            finally
            {
                await _imapService.DisconnectAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error syncing account {Email}", account.Email);
        }
    }

    public async Task MarkAsReadAsync(int messageId)
    {
        var message = await _emailRepository.GetMessageByIdAsync(messageId);
        if (message == null) return;

        message.Flags |= Core.Enums.EmailFlag.Seen;
        await _emailRepository.UpdateMessageAsync(message);

        var folder = await _emailRepository.GetFolderByIdAsync(message.FolderId);
        if (folder != null)
        {
            await _imapService.UpdateFlagsAsync(folder, message.Uid!, message.Flags);
        }
    }

    public async Task MarkAsUnreadAsync(int messageId)
    {
        var message = await _emailRepository.GetMessageByIdAsync(messageId);
        if (message == null) return;

        message.Flags &= ~Core.Enums.EmailFlag.Seen;
        await _emailRepository.UpdateMessageAsync(message);

        var folder = await _emailRepository.GetFolderByIdAsync(message.FolderId);
        if (folder != null)
        {
            await _imapService.UpdateFlagsAsync(folder, message.Uid!, message.Flags);
        }
    }

    public async Task ToggleFlagAsync(int messageId)
    {
        var message = await _emailRepository.GetMessageByIdAsync(messageId);
        if (message == null) return;

        if (message.Flags.HasFlag(Core.Enums.EmailFlag.Flagged))
            message.Flags &= ~Core.Enums.EmailFlag.Flagged;
        else
            message.Flags |= Core.Enums.EmailFlag.Flagged;

        await _emailRepository.UpdateMessageAsync(message);

        var folder = await _emailRepository.GetFolderByIdAsync(message.FolderId);
        if (folder != null)
        {
            await _imapService.UpdateFlagsAsync(folder, message.Uid!, message.Flags);
        }
    }

    public async Task DeleteMessageAsync(int messageId)
    {
        var message = await _emailRepository.GetMessageByIdAsync(messageId);
        if (message == null) return;

        var folder = await _emailRepository.GetFolderByIdAsync(message.FolderId);
        if (folder != null)
        {
            message.Flags |= Core.Enums.EmailFlag.Deleted;
            await _emailRepository.UpdateMessageAsync(message);
            await _imapService.UpdateFlagsAsync(folder, message.Uid!, message.Flags);
        }

        await _emailRepository.SoftDeleteMessageAsync(messageId);
    }
}
