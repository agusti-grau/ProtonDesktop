using ProtonDesktop.Core.Interfaces;
using Serilog;
namespace ProtonDesktop.Services;

public interface IBackgroundSyncService
{
    Task StartAsync(int intervalMinutes = 5);
    Task StopAsync();
    bool IsRunning { get; }
    event EventHandler<SyncProgressEventArgs> SyncStarted;
    event EventHandler<SyncProgressEventArgs> SyncCompleted;
    event EventHandler<SyncErrorEventArgs> SyncError;
}

public class SyncProgressEventArgs : EventArgs
{
    public DateTime StartTime { get; init; }
    public string Status { get; init; } = string.Empty;
}

public class SyncErrorEventArgs : EventArgs
{
    public string ErrorMessage { get; init; } = string.Empty;
    public Exception? Exception { get; init; }
}

public class BackgroundSyncService : IBackgroundSyncService
{
    private readonly IEmailRepository _emailRepository;
    private readonly ICalendarRepository _calendarRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IImapSyncService _imapSyncService;
    private readonly ISmtpService _smtpService;
    private readonly ICalDavSyncService _calDavSyncService;
    private readonly ILogger _logger;
    private Timer? _syncTimer;
    private bool _isSyncing;
    private int _intervalMinutes = 5;

    public bool IsRunning => _syncTimer != null;

    public event EventHandler<SyncProgressEventArgs>? SyncStarted;
    public event EventHandler<SyncProgressEventArgs>? SyncCompleted;
    public event EventHandler<SyncErrorEventArgs>? SyncError;

    public BackgroundSyncService(
        IEmailRepository emailRepository,
        ICalendarRepository calendarRepository,
        IAccountRepository accountRepository,
        IImapSyncService imapSyncService,
        ISmtpService smtpService,
        ICalDavSyncService calDavSyncService,
        ILogger logger)
    {
        _emailRepository = emailRepository;
        _calendarRepository = calendarRepository;
        _accountRepository = accountRepository;
        _imapSyncService = imapSyncService;
        _smtpService = smtpService;
        _calDavSyncService = calDavSyncService;
        _logger = logger;
    }

    public Task StartAsync(int intervalMinutes = 5)
    {
        if (_syncTimer != null)
        {
            _logger.Warning("Background sync service is already running");
            return Task.CompletedTask;
        }

        _intervalMinutes = intervalMinutes;
        _syncTimer = new Timer(SyncCallback, null, TimeSpan.Zero, TimeSpan.FromMinutes(_intervalMinutes));
        _logger.Information("Background sync service started with interval {Interval} minutes", _intervalMinutes);
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        if (_syncTimer == null)
        {
            _logger.Warning("Background sync service is not running");
            return Task.CompletedTask;
        }

        _syncTimer.Dispose();
        _syncTimer = null;
        _logger.Information("Background sync service stopped");
        return Task.CompletedTask;
    }

    private async void SyncCallback(object? state)
    {
        if (_isSyncing)
        {
            _logger.Debug("Sync already in progress, skipping");
            return;
        }

        _isSyncing = true;
        var startTime = DateTime.UtcNow;

        try
        {
            SyncStarted?.Invoke(this, new SyncProgressEventArgs
            {
                StartTime = startTime,
                Status = "Starting sync"
            });

            var accounts = await _accountRepository.GetAllAccountsAsync();
            foreach (var account in accounts)
            {
                try
                {
                    await SyncAccountAsync(account);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error syncing account {Email}", account.Email);
                    SyncError?.Invoke(this, new SyncErrorEventArgs
                    {
                        ErrorMessage = $"Error syncing account {account.Email}",
                        Exception = ex
                    });
                }
            }

            SyncCompleted?.Invoke(this, new SyncProgressEventArgs
            {
                StartTime = startTime,
                Status = "Sync completed"
            });

            _logger.Information("Background sync completed in {Duration}ms", (DateTime.UtcNow - startTime).TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Background sync failed");
            SyncError?.Invoke(this, new SyncErrorEventArgs
            {
                ErrorMessage = "Background sync failed",
                Exception = ex
            });
        }
        finally
        {
            _isSyncing = false;
        }
    }

    private async Task SyncAccountAsync(Core.Models.MailAccount account)
    {
        _logger.Information("Syncing account {Email}", account.Email);

        if (!await _imapSyncService.ConnectAsync(account))
        {
            _logger.Error("Failed to connect to IMAP for account {Email}", account.Email);
            return;
        }

        try
        {
            var folders = await _imapSyncService.SyncFoldersAsync(account);
            foreach (var folder in folders)
            {
                var existingFolder = await _emailRepository.GetFolderByPathAsync(account.Id, folder.Path);

                Core.Models.EmailFolder targetFolder;
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

                var newMessages = await _imapSyncService.SyncNewMessagesAsync(targetFolder, lastUid);
                foreach (var message in newMessages)
                {
                    var existingMessage = await _emailRepository.GetMessageByUidAsync(targetFolder.Id, message.Uid!);
                    if (existingMessage == null)
                    {
                        message.FolderId = targetFolder.Id;
                        await _emailRepository.CreateMessageAsync(message);

                        if (message.HasAttachments)
                        {
                            var attachments = await _imapSyncService.DownloadAttachmentsAsync(message, targetFolder);
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

            var calendars = await _calDavSyncService.SyncCalendarsAsync(account);
            foreach (var calendar in calendars)
            {
                var existingCalendar = await _calendarRepository.GetCalendarByIdAsync(calendar.Id);
                if (existingCalendar == null)
                {
                    calendar.MailAccountId = account.Id;
                    await _calendarRepository.CreateCalendarAsync(calendar);
                }
                else
                {
                    var events = await _calDavSyncService.SyncEventsAsync(calendar);
                    foreach (var calendarEvent in events)
                    {
                        var existingEvent = await _calendarRepository.GetEventByUidAsync(calendar.Id, calendarEvent.Uid);
                        if (existingEvent == null)
                        {
                            calendarEvent.CalendarId = calendar.Id;
                            await _calendarRepository.CreateEventAsync(calendarEvent);
                        }
                        else
                        {
                            existingEvent.Title = calendarEvent.Title;
                            existingEvent.Description = calendarEvent.Description;
                            existingEvent.Location = calendarEvent.Location;
                            existingEvent.StartUtc = calendarEvent.StartUtc;
                            existingEvent.EndUtc = calendarEvent.EndUtc;
                            existingEvent.IsAllDay = calendarEvent.IsAllDay;
                            existingEvent.ETag = calendarEvent.ETag;
                            existingEvent.UpdatedAt = DateTime.UtcNow;
                            await _calendarRepository.UpdateEventAsync(existingEvent);
                        }
                    }
                }
            }

            account.LastSyncAt = DateTime.UtcNow;
            await _accountRepository.UpdateAccountAsync(account);

            _logger.Information("Sync completed for account {Email}", account.Email);
        }
        finally
        {
            await _imapSyncService.DisconnectAsync();
        }
    }
}
