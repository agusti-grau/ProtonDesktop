using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonDesktop.Core.Interfaces;
using ProtonDesktop.Core.Models;
using ProtonDesktop.Services;
using ProtonDesktop.Services.Email;
using ProtonDesktop.Views;
using Serilog;

namespace ProtonDesktop.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IAccountRepository _accountRepository;
    private readonly IEmailRepository _emailRepository;
    private readonly IImapSyncService _imapSyncService;
    private readonly IBackgroundSyncService _backgroundSyncService;
    private readonly EmailSendService _emailSendService;
    private readonly ILogger _logger;

    [ObservableProperty]
    private string _title = "ProtonDesktop";

    [ObservableProperty]
    private bool _isSyncing;

    [ObservableProperty]
    private string _syncStatus = "Ready";

    [ObservableProperty]
    private FolderTreeViewModel? _folderTree;

    [ObservableProperty]
    private EmailListViewModel? _emailList;

    [ObservableProperty]
    private ReadingPaneViewModel? _readingPane;

    public MainViewModel(
        IAccountRepository accountRepository,
        IEmailRepository emailRepository,
        IImapSyncService imapSyncService,
        IBackgroundSyncService backgroundSyncService,
        EmailSendService emailSendService,
        ILogger logger)
    {
        _accountRepository = accountRepository;
        _emailRepository = emailRepository;
        _imapSyncService = imapSyncService;
        _backgroundSyncService = backgroundSyncService;
        _emailSendService = emailSendService;
        _logger = logger;

        _backgroundSyncService.SyncStarted += OnSyncStarted;
        _backgroundSyncService.SyncCompleted += OnSyncCompleted;
        _backgroundSyncService.SyncError += OnSyncError;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        try
        {
            _logger.Information("Loading main view");

            var defaultAccount = await _accountRepository.GetDefaultAccountAsync();
            if (defaultAccount == null)
            {
                SyncStatus = "No account configured";
                return;
            }

            FolderTree = new FolderTreeViewModel(_emailRepository, defaultAccount.Id);
            await FolderTree.LoadAsync();

            EmailList = new EmailListViewModel(_emailRepository);
            ReadingPane = new ReadingPaneViewModel(_emailRepository);

            if (FolderTree.Folders.Any())
            {
                var inbox = FolderTree.Folders.FirstOrDefault(f => f.FolderType == Core.Enums.FolderType.Inbox);
                if (inbox != null)
                {
                    await SelectFolderAsync(inbox);
                }
            }

            SyncStatus = "Ready";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading main view");
            SyncStatus = "Error loading";
        }
    }

    [RelayCommand]
    private async Task SyncAsync()
    {
        try
        {
            IsSyncing = true;
            SyncStatus = "Syncing...";

            var defaultAccount = await _accountRepository.GetDefaultAccountAsync();
            if (defaultAccount == null)
            {
                SyncStatus = "No account configured";
                return;
            }

            await _backgroundSyncService.StartAsync(1);
            await Task.Delay(1000);
            await _backgroundSyncService.StopAsync();

            if (FolderTree != null)
            {
                await FolderTree.LoadAsync();
            }

            SyncStatus = "Sync complete";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error syncing");
            SyncStatus = "Sync failed";
        }
        finally
        {
            IsSyncing = false;
        }
    }

    [RelayCommand]
    private async Task NewEmailAsync()
    {
        var composeViewModel = new ComposeViewModel(_accountRepository, _emailSendService)
        {
            Mode = ComposeMode.New
        };

        var composeWindow = new ComposeWindow();
        composeWindow.Owner = System.Windows.Application.Current.MainWindow;
        await composeWindow.InitializeAsync(composeViewModel);
        composeWindow.ShowDialog();
    }

    [RelayCommand]
    private async Task ReplyAsync()
    {
        if (ReadingPane == null || ReadingPane.MessageId == 0) return;

        var message = await _emailRepository.GetMessageByIdAsync(ReadingPane.MessageId);
        if (message == null) return;

        var composeViewModel = new ComposeViewModel(_accountRepository, _emailSendService)
        {
            Mode = ComposeMode.Reply,
            OriginalMessage = message
        };

        var composeWindow = new ComposeWindow();
        composeWindow.Owner = System.Windows.Application.Current.MainWindow;
        composeWindow.Title = $"Re: {message.Subject}";
        await composeWindow.InitializeAsync(composeViewModel);
        composeWindow.ShowDialog();
    }

    [RelayCommand]
    private async Task ForwardAsync()
    {
        if (ReadingPane == null || ReadingPane.MessageId == 0) return;

        var message = await _emailRepository.GetMessageByIdAsync(ReadingPane.MessageId);
        if (message == null) return;

        var composeViewModel = new ComposeViewModel(_accountRepository, _emailSendService)
        {
            Mode = ComposeMode.Forward,
            OriginalMessage = message
        };

        var composeWindow = new ComposeWindow();
        composeWindow.Owner = System.Windows.Application.Current.MainWindow;
        composeWindow.Title = $"Fw: {message.Subject}";
        await composeWindow.InitializeAsync(composeViewModel);
        composeWindow.ShowDialog();
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        if (ReadingPane == null || ReadingPane.MessageId == 0) return;

        var result = System.Windows.MessageBox.Show(
            "Are you sure you want to delete this message?",
            "Confirm Delete",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            await ReadingPane.DeleteCommand.ExecuteAsync(null);

            if (EmailList != null)
            {
                var message = EmailList.Messages.FirstOrDefault(m => m.Id == ReadingPane.MessageId);
                if (message != null)
                {
                    EmailList.Messages.Remove(message);
                }
            }
        }
    }

    public async Task SelectFolderAsync(FolderViewModel folder)
    {
        if (EmailList == null) return;

        await EmailList.LoadMessagesAsync(folder.Id);

        if (EmailList.Messages.Any())
        {
            await SelectMessageAsync(EmailList.Messages.First());
        }
        else
        {
            ReadingPane?.Clear();
        }
    }

    public async Task SelectMessageAsync(EmailMessageViewModel message)
    {
        if (ReadingPane == null) return;

        await ReadingPane.LoadMessageAsync(message.Id);

        if (!message.IsRead)
        {
            message.IsRead = true;
            var emailMessage = await _emailRepository.GetMessageByIdAsync(message.Id);
            if (emailMessage != null)
            {
                emailMessage.Flags |= Core.Enums.EmailFlag.Seen;
                await _emailRepository.UpdateMessageAsync(emailMessage);
            }
        }
    }

    private void OnSyncStarted(object? sender, SyncProgressEventArgs e)
    {
        IsSyncing = true;
        SyncStatus = e.Status;
    }

    private void OnSyncCompleted(object? sender, SyncProgressEventArgs e)
    {
        IsSyncing = false;
        SyncStatus = e.Status;
    }

    private void OnSyncError(object? sender, SyncErrorEventArgs e)
    {
        IsSyncing = false;
        SyncStatus = "Sync error";
        _logger.Error(e.Exception, "Sync error: {Message}", e.ErrorMessage);
    }
}
