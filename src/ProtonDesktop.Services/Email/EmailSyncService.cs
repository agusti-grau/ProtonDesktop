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

            var folders = await _imapService.SyncFoldersAsync(account);
            foreach (var folder in folders)
            {
                var existingFolder = await _emailRepository.GetFolderByPathAsync(account.Id, folder.Path);
                if (existingFolder == null)
                {
                    folder.MailAccountId = account.Id;
                    await _emailRepository.CreateFolderAsync(folder);
                }
                else
                {
                    existingFolder.UidNext = folder.UidNext;
                    existingFolder.UidValidity = folder.UidValidity;
                    existingFolder.LastSyncAt = DateTime.UtcNow;
                    await _emailRepository.UpdateFolderAsync(existingFolder);
                }

                var messages = await _imapService.SyncMessagesAsync(folder);
                foreach (var message in messages)
                {
                    var existingMessage = await _emailRepository.GetMessageByUidAsync(folder.Id, message.Uid!);
                    if (existingMessage == null)
                    {
                        message.FolderId = folder.Id;
                        await _emailRepository.CreateMessageAsync(message);
                    }
                }
            }

            await _emailRepository.UpdateUnreadCountsAsync(account.Id);

            account.LastSyncAt = DateTime.UtcNow;
            await _accountRepository.UpdateAccountAsync(account);

            await _imapService.DisconnectAsync();

            _logger.Information("Sync completed for account {Email}", account.Email);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error syncing account {Email}", account.Email);
        }
    }
}
