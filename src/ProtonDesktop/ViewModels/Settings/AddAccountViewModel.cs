using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonDesktop.Core.Interfaces;
using ProtonDesktop.Core.Models;
using Serilog;

namespace ProtonDesktop.ViewModels.Settings;

public partial class AddAccountViewModel : ObservableObject
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICredentialStore _credentialStore;
    private readonly IImapSyncService _imapSyncService;
    private readonly ILogger _logger;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _imapHost = "127.0.0.1";

    [ObservableProperty]
    private int _imapPort = 1143;

    [ObservableProperty]
    private string _smtpHost = "127.0.0.1";

    [ObservableProperty]
    private int _smtpPort = 1025;

    [ObservableProperty]
    private string _caldavHost = "127.0.0.1";

    [ObservableProperty]
    private int _caldavPort = 8080;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _testPassed;

    public AddAccountViewModel(
        IAccountRepository accountRepository,
        ICredentialStore credentialStore,
        IImapSyncService imapSyncService,
        ILogger logger)
    {
        _accountRepository = accountRepository;
        _credentialStore = credentialStore;
        _imapSyncService = imapSyncService;
        _logger = logger;
    }

    public event EventHandler<bool>? RequestClose;

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        if (!Validate(out var error))
        {
            StatusMessage = error;
            return;
        }

        try
        {
            IsBusy = true;
            TestPassed = false;
            StatusMessage = "Testing connection...";

            var account = BuildAccount();
            var connected = await _imapSyncService.ConnectAsync(account);

            if (connected)
            {
                await _imapSyncService.DisconnectAsync();
                TestPassed = true;
                StatusMessage = "Connection successful";
            }
            else
            {
                StatusMessage = "Connection failed. Check host, port and credentials.";
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Connection test failed");
            StatusMessage = $"Connection failed: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!Validate(out var error))
        {
            StatusMessage = error;
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Saving account...";

            var account = BuildAccount();
            account.IsDefault = !(await _accountRepository.GetAllAccountsAsync()).Any();

            await _accountRepository.CreateAccountAsync(account);

            _logger.Information("Account {Email} added", account.Email);
            RequestClose?.Invoke(this, true);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error saving account");
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        RequestClose?.Invoke(this, false);
    }

    private bool Validate(out string error)
    {
        if (string.IsNullOrWhiteSpace(Email) || !Email.Contains('@'))
        {
            error = "Enter a valid email address";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            error = "Enter the Bridge password (from Bridge > Mailbox details)";
            return false;
        }

        if (string.IsNullOrWhiteSpace(DisplayName))
        {
            error = "Enter a display name";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private MailAccount BuildAccount()
    {
        return new MailAccount
        {
            Email = Email.Trim(),
            DisplayName = DisplayName.Trim(),
            ImapHost = ImapHost.Trim(),
            ImapPort = ImapPort,
            SmtpHost = SmtpHost.Trim(),
            SmtpPort = SmtpPort,
            CalDavHost = CaldavHost.Trim(),
            CalDavPort = CaldavPort,
            EncryptedPassword = _credentialStore.Encrypt(Password)
        };
    }
}
