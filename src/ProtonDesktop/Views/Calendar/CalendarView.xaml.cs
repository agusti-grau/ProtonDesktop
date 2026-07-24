using System.Windows;
using ProtonDesktop.ViewModels.Calendar;

namespace ProtonDesktop.Views.Calendar;

public partial class CalendarView : Window
{
    private readonly CalendarViewModel _viewModel;

    public CalendarView(CalendarViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
        Loaded += CalendarView_Loaded;
    }

    private async void CalendarView_Loaded(object sender, RoutedEventArgs e)
    {
        await _viewModel.LoadCommand.ExecuteAsync(null);
    }

    private async void NewEvent_Click(object sender, RoutedEventArgs e)
    {
        var eventEditor = new EventEditorWindow();
        eventEditor.Owner = this;
        await eventEditor.ViewModel.LoadCommand.ExecuteAsync(null);
        eventEditor.ShowDialog();

        if (eventEditor.ViewModel.SavedSuccessfully)
        {
            await _viewModel.LoadEventsCommand.ExecuteAsync(null);
        }
    }
}
