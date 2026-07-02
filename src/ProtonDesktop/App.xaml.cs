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
using Serilog;

namespace ProtonDesktop;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
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

        services.AddTransient<EmailSyncService>();
        services.AddTransient<EmailSendService>();
        services.AddTransient<CalendarSyncService>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information("ProtonDesktop exiting");
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
