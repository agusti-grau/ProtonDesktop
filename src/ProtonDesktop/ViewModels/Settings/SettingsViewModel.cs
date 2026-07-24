using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ProtonDesktop.Core.Interfaces;
using ProtonDesktop.Core.Models;
using Serilog;
using System.Collections.ObjectModel;

namespace ProtonDesktop.ViewModels.Settings;

public partial class SettingsViewModel : ObservableObject
{
    private readonly IAccountRepository _accountRepository;
    private readonly ICredentialStore _credentialStore;
    private readonly ILogger _logger;

    [ObservableProperty]
    private ObservableCollection<MailAccount> _accounts = new();

    [ObservableProperty]
    private MailAccount? _selectedAccount;

    [ObservableProperty]
    private string _imapHost = "localhost";

    [ObservableProperty]
    private int _imapPort = 1143;

    [ObservableProperty]
    private string _smtpHost = "localhost";

    [ObservableProperty]
    private int _smtpPort = 1025;

    [ObservableProperty]
    private string _caldavHost = "localhost";

    [ObservableProperty]
    private int _caldavPort = 8080;

    [ObservableProperty]
    private int _syncIntervalMinutes = 5;

    [ObservableProperty]
    private bool _startMinimized;

    [ObservableProperty]
    private bool _minimizeToTray = true;

    [ObservableProperty]
    private bool _showNotifications = true;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public SettingsViewModel(
        IAccountRepository accountRepository,
        ICredentialStore credentialStore,
        ILogger logger)
    {
        _accountRepository = accountRepository;
        _credentialStore = credentialStore;
        _logger = logger;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        try
        {
            var accounts = await _accountRepository.GetAllAccountsAsync();
            Accounts.Clear();
            foreach (var account in accounts)
            {
                Accounts.Add(account);
            }

            if (Accounts.Any())
            {
                SelectedAccount = Accounts.First();
                LoadAccountSettings(SelectedAccount);
            }

            StatusMessage = "Settings loaded";
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error loading settings");
            StatusMessage = "Error loading settings";
        }
    }

    private void LoadAccountSettings(MailAccount account)
    {
        ImapHost = account.ImapHost;
        ImapPort = account.ImapPort;
        SmtpHost = account.SmtpHost;
        SmtpPort = account.SmtpPort;
        CaldavHost = account.CalDavHost;
        CaldavPort = account.CalDavPort;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        try
        {
            if (SelectedAccount == null)
            {
                StatusMessage = "No account selected";
                return;
            }

            SelectedAccount.ImapHost = ImapHost;
            SelectedAccount.ImapPort = ImapPort;
            SelectedAccount.SmtpHost = SmtpHost;
            SelectedAccount.SmtpPort = SmtpPort;
            SelectedAccount.CalDavHost = CaldavHost;
            SelectedAccount.CalDavPort = CaldavPort;

            await _accountRepository.UpdateAccountAsync(SelectedAccount);

            StatusMessage = "Settings saved successfully";
            _logger.Information("Settings saved for account {Email}", SelectedAccount.Email);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error saving settings");
            StatusMessage = "Error saving settings";
        }
    }

    [RelayCommand]
    private async Task AddAccountAsync()
    {
        try
        {
            var newAccount = new MailAccount
            {
                Email = "user@proton.me",
                DisplayName = "User",
                ImapHost = ImapHost,
                ImapPort = ImapPort,
                SmtpHost = SmtpHost,
                SmtpPort = SmtpPort,
                CalDavHost = CaldavHost,
                CalDavPort = CaldavPort,
                EncryptedPassword = _credentialStore.Encrypt("password"),
                IsDefault = !Accounts.Any()
            };

            var createdAccount = await _accountRepository.CreateAccountAsync(newAccount);
            Accounts.Add(createdAccount);
            SelectedAccount = createdAccount;

            StatusMessage = "Account added successfully";
            _logger.Information("Added new account {Email}", newAccount.Email);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error adding account");
            StatusMessage = "Error adding account";
        }
    }

    [RelayCommand]
    private async Task RemoveAccountAsync()
    {
        try
        {
            if (SelectedAccount == null)
            {
                StatusMessage = "No account selected";
                return;
            }

            await _accountRepository.DeleteAccountAsync(SelectedAccount.Id);
            Accounts.Remove(SelectedAccount);
            SelectedAccount = Accounts.FirstOrDefault();

            if (SelectedAccount != null)
            {
                LoadAccountSettings(SelectedAccount);
            }

            StatusMessage = "Account removed successfully";
            _logger.Information("Removed account");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error removing account");
            StatusMessage = "Error removing account";
        }
    }

    [RelayCommand]
    private async Task SetDefaultAccountAsync()
    {
        try
        {
            if (SelectedAccount == null)
            {
                StatusMessage = "No account selected";
                return;
            }

            await _accountRepository.SetDefaultAccountAsync(SelectedAccount.Id);

            foreach (var account in Accounts)
            {
                account.IsDefault = account.Id == SelectedAccount.Id;
            }

            StatusMessage = "Default account set";
            _logger.Information("Set default account to {Email}", SelectedAccount.Email);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error setting default account");
            StatusMessage = "Error setting default account";
        }
    }

    partial void OnSelectedAccountChanged(MailAccount? value)
    {
        if (value != null)
        {
            LoadAccountSettings(value);
        }
    }
}
