using System.Windows;
using ProtonDesktop.ViewModels;

namespace ProtonDesktop.Views;

public partial class ComposeWindow : Window
{
    public ComposeWindow()
    {
        InitializeComponent();
    }

    public async Task InitializeAsync(ComposeViewModel viewModel)
    {
        DataContext = viewModel;
        viewModel.RequestClose += (s, e) => Close();
        await viewModel.InitializeAsync();
    }
}
