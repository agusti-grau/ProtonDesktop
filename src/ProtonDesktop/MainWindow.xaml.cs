using System.Windows;
using System.Windows.Controls;
using ProtonDesktop.ViewModels;

namespace ProtonDesktop;

public partial class MainWindow : Window
{
    private MainViewModel? _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
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
}
