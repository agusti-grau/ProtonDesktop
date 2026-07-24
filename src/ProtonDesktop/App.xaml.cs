using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProtonDesktop.Core.Interfaces;
using ProtonDesktop.Infrastructure.Data;
using ProtonDesktop.Infrastructure.Protocols;
using ProtonDesktop.Infrastructure.Repositories;
using ProtonDesktop.Infrastructure.Security;
using ProtonDesktop.Services;
using ProtonDesktop.Services.Calendar;
using ProtonDesktop.Services.Email;
using ProtonDesktop.Services.Navigation;
using ProtonDesktop.Services.Notifications;
using ProtonDesktop.ViewModels;
using Serilog;

namespace ProtonDesktop;

public partial class App : System.Windows.Application
{
    public static IServiceProvider Services { get; private set; } = null!;
    private ISystemTrayService? _systemTrayService;
    private IBackgroundSyncService? _backgroundSyncService;
    private IReminderService? _reminderService;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Debug()
            .WriteTo.File("logs/protondesktop-.log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        Log.Information("ProtonDesktop starting");

        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        using (var scope = Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.Migrate();
        }

        _systemTrayService = Services.GetService<ISystemTrayService>();
        _systemTrayService?.Initialize();

        _backgroundSyncService = Services.GetService<IBackgroundSyncService>();
        if (_backgroundSyncService != null)
        {
            _backgroundSyncService.SyncCompleted += OnSyncCompleted;
            await _backgroundSyncService.StartAsync(5);
        }

        _reminderService = Services.GetService<IReminderService>();
        if (_reminderService != null)
        {
            _reminderService.ReminderTriggered += OnReminderTriggered;
            await _reminderService.StartAsync();
        }

        var mainWindow = new MainWindow();
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite("Data Source=protondesktop.db"));

        services.AddSingleton<ILogger>(sp => Log.Logger);

        services.AddSingleton<IAccountRepository, AccountRepository>();
        services.AddSingleton<IEmailRepository, EmailRepository>();
        services.AddSingleton<ICalendarRepository, CalendarRepository>();

        services.AddTransient<IImapSyncService, ImapSyncService>();
        services.AddTransient<ISmtpService, SmtpService>();
        services.AddTransient<ICalDavSyncService, CalDavSyncService>();

        services.AddSingleton<ICredentialStore, CredentialStore>();
        services.AddSingleton<IBackgroundSyncService, BackgroundSyncService>();

        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IReminderService, ReminderService>();
        services.AddSingleton<ISystemTrayService, SystemTrayService>();
        services.AddSingleton<IKeyboardShortcutService, KeyboardShortcutService>();

        services.AddTransient<EmailSyncService>();
        services.AddTransient<EmailSendService>();
        services.AddTransient<CalendarSyncService>();

        services.AddTransient<MainViewModel>();
        services.AddTransient<ProtonDesktop.ViewModels.Calendar.CalendarViewModel>();
        services.AddTransient<ProtonDesktop.ViewModels.Calendar.EventEditorViewModel>();
        services.AddTransient<ProtonDesktop.ViewModels.Settings.SettingsViewModel>();
    }

    private void OnSyncCompleted(object? sender, SyncProgressEventArgs e)
    {
        Dispatcher.Invoke(async () =>
        {
            var emailRepo = Services.GetService<IEmailRepository>();
            var accountRepo = Services.GetService<IAccountRepository>();
            if (emailRepo != null && accountRepo != null)
            {
                var account = await accountRepo.GetDefaultAccountAsync();
                if (account != null)
                {
                    var folders = await emailRepo.GetFoldersAsync(account.Id);
                    var totalUnread = folders.Sum(f => f.UnreadCount);
                    _systemTrayService?.UpdateUnreadCount(totalUnread);
                }
            }
        });
    }

    private void OnReminderTriggered(object? sender, ReminderEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            _systemTrayService?.ShowNotification(
                "Calendar Reminder",
                $"{e.EventTitle} starts at {e.EventStart.ToLocalTime():HH:mm}");
        });
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        Log.Information("ProtonDesktop exiting");

        if (_backgroundSyncService != null)
        {
            await _backgroundSyncService.StopAsync();
        }

        if (_reminderService != null)
        {
            await _reminderService.StopAsync();
        }

        if (_systemTrayService is IDisposable disposable)
        {
            disposable.Dispose();
        }

        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
