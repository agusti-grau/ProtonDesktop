using System.Windows;
using ProtonDesktop.ViewModels.Calendar;

namespace ProtonDesktop.Views.Calendar;

public partial class EventEditorWindow : Window
{
    public EventEditorViewModel ViewModel { get; }

    public EventEditorWindow()
    {
        InitializeComponent();
        ViewModel = App.Services.GetService(typeof(EventEditorViewModel)) as EventEditorViewModel 
            ?? new EventEditorViewModel(
                App.Services.GetService(typeof(ProtonDesktop.Core.Interfaces.ICalendarRepository)) as ProtonDesktop.Core.Interfaces.ICalendarRepository,
                App.Services.GetService(typeof(ProtonDesktop.Core.Interfaces.IAccountRepository)) as ProtonDesktop.Core.Interfaces.IAccountRepository,
                App.Services.GetService(typeof(Serilog.ILogger)) as Serilog.ILogger);
        DataContext = ViewModel;
        ViewModel.RequestClose += ViewModel_RequestClose;
        Loaded += EventEditorWindow_Loaded;
    }

    private async void EventEditorWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await ViewModel.LoadCommand.ExecuteAsync(null);
    }

    private void ViewModel_RequestClose(object? sender, bool savedSuccessfully)
    {
        SavedSuccessfully = savedSuccessfully;
        Close();
    }

    public bool SavedSuccessfully { get; private set; }
}
