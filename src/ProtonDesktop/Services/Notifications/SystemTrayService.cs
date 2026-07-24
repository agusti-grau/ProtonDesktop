using System.Windows;
using System.Windows.Forms;
using Serilog;

namespace ProtonDesktop.Services.Notifications;

public interface ISystemTrayService
{
    void Initialize();
    void ShowNotification(string title, string message);
    void UpdateUnreadCount(int count);
    event EventHandler? ShowWindowRequested;
}

public class SystemTrayService : ISystemTrayService, IDisposable
{
    private readonly ILogger _logger;
    private NotifyIcon? _notifyIcon;
    private bool _disposed;

    public event EventHandler? ShowWindowRequested;

    public SystemTrayService(ILogger logger)
    {
        _logger = logger;
    }

    public void Initialize()
    {
        try
        {
            _notifyIcon = new NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Application,
                Visible = true,
                Text = "ProtonDesktop"
            };

            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("Show", null, OnShowClick);
            contextMenu.Items.Add("Check for New Mail", null, OnCheckMailClick);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add("Exit", null, OnExitClick);

            _notifyIcon.ContextMenuStrip = contextMenu;
            _notifyIcon.DoubleClick += OnNotifyIconDoubleClick;
            _notifyIcon.BalloonTipClicked += OnBalloonTipClicked;

            _logger.Information("System tray initialized");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error initializing system tray");
        }
    }

    public void ShowNotification(string title, string message)
    {
        try
        {
            _notifyIcon?.ShowBalloonTip(
                3000,
                title,
                message,
                ToolTipIcon.Info);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error showing notification");
        }
    }

    public void UpdateUnreadCount(int count)
    {
        try
        {
            if (_notifyIcon != null)
            {
                _notifyIcon.Text = count > 0 
                    ? $"ProtonDesktop ({count} unread)" 
                    : "ProtonDesktop";
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error updating unread count");
        }
    }

    private void OnShowClick(object? sender, EventArgs e)
    {
        ShowWindowRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnCheckMailClick(object? sender, EventArgs e)
    {
        _logger.Information("Check mail requested from system tray");
    }

    private void OnExitClick(object? sender, EventArgs e)
    {
        _logger.Information("Exit requested from system tray");
        System.Windows.Application.Current.Shutdown();
    }

    private void OnNotifyIconDoubleClick(object? sender, EventArgs e)
    {
        ShowWindowRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnBalloonTipClicked(object? sender, EventArgs e)
    {
        ShowWindowRequested?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _notifyIcon?.Dispose();
            _disposed = true;
        }
    }
}
