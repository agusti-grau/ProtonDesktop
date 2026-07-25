using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ProtonDesktop.Services;
using ProtonDesktop.Services.Notifications;
using ProtonDesktop.ViewModels;
using ProtonDesktop.ViewModels.Calendar;
using ProtonDesktop.Views.Calendar;
using ProtonDesktop.Views.Settings;
using Serilog;

namespace ProtonDesktop;

public partial class MainWindow : Window
{
    private MainViewModel? _viewModel;
    private readonly ISystemTrayService? _systemTrayService;
    private readonly IKeyboardShortcutService? _keyboardShortcutService;
    private readonly ILogger _logger;

    public MainWindow()
    {
        InitializeComponent();
        _logger = App.Services.GetService(typeof(ILogger)) as ILogger ?? Log.Logger;
        _systemTrayService = App.Services.GetService(typeof(ISystemTrayService)) as ISystemTrayService;
        _keyboardShortcutService = App.Services.GetService(typeof(IKeyboardShortcutService)) as IKeyboardShortcutService;

        Loaded += MainWindow_Loaded;
        StateChanged += MainWindow_StateChanged;

        if (_systemTrayService != null)
        {
            _systemTrayService.ShowWindowRequested += OnShowWindowRequested;
        }

        RegisterKeyboardShortcuts();
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _viewModel = App.Services.GetService(typeof(MainViewModel)) as MainViewModel;
        if (_viewModel != null)
        {
            DataContext = _viewModel;
            await _viewModel.LoadAsync();
        }
    }

    private void RegisterKeyboardShortcuts()
    {
        if (_keyboardShortcutService == null) return;

        _keyboardShortcutService.RegisterShortcut(Key.N, ModifierKeys.Control, () =>
        {
            _viewModel?.NewEmailCommand.Execute(null);
        });

        _keyboardShortcutService.RegisterShortcut(Key.R, ModifierKeys.Control, () =>
        {
            _viewModel?.ReplyCommand.Execute(null);
        });

        _keyboardShortcutService.RegisterShortcut(Key.F, ModifierKeys.Control, () =>
        {
            _viewModel?.ForwardCommand.Execute(null);
        });

        _keyboardShortcutService.RegisterShortcut(Key.Delete, ModifierKeys.None, () =>
        {
            _viewModel?.DeleteCommand.Execute(null);
        });

        _keyboardShortcutService.RegisterShortcut(Key.F5, ModifierKeys.None, () =>
        {
            _viewModel?.SyncCommand.Execute(null);
        });
    }

    protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (_keyboardShortcutService?.HandleKeyDown(e.Key, Keyboard.Modifiers) == true)
        {
            e.Handled = true;
        }
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        if (WindowState == WindowState.Minimized)
        {
            Hide();
            _systemTrayService?.ShowNotification("ProtonDesktop", "Running in background");
        }
    }

    private void OnShowWindowRequested(object? sender, EventArgs e)
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
        Focus();
    }

    private async void NavMail_Click(object sender, RoutedEventArgs e)
    {
        MailContent.Visibility = Visibility.Visible;

        // Reload if the view model was created before an account existed
        if (_viewModel?.FolderTree == null)
        {
            await _viewModel.LoadAsync();
        }
    }

    private void NavCalendar_Click(object sender, RoutedEventArgs e)
    {
        var calendarViewModel = App.Services.GetService(typeof(CalendarViewModel)) as CalendarViewModel;
        if (calendarViewModel != null)
        {
            var calendarWindow = new CalendarView(calendarViewModel)
            {
                Owner = this
            };
            calendarWindow.ShowDialog();
        }
    }

    private async void NavSettings_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsView
        {
            Owner = this
        };
        settingsWindow.ShowDialog();

        // Reload after settings closed - account may have been added/changed
        if (_viewModel != null)
        {
            await _viewModel.LoadAsync();
        }
    }

    private async void FolderTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
        if (e.NewValue is TreeViewItem treeViewItem && treeViewItem.DataContext is FolderViewModel folder)
        {
            if (_viewModel != null)
            {
                await _viewModel.SelectFolderAsync(folder);
            }
        }
    }

    private async void EmailList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is EmailMessageViewModel message)
        {
            if (_viewModel != null)
            {
                await _viewModel.SelectMessageAsync(message);
            }
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        Hide();
        _systemTrayService?.ShowNotification("ProtonDesktop", "Running in background. Right-click tray icon to exit.");
    }
}
