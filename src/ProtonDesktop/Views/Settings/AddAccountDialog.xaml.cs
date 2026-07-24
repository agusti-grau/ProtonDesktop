using System.Windows;
using System.Windows.Controls;
using ProtonDesktop.ViewModels.Settings;

namespace ProtonDesktop.Views.Settings;

public partial class AddAccountDialog : Window
{
    public bool Saved { get; private set; }

    public AddAccountDialog(AddAccountViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.RequestClose += OnRequestClose;
    }

    private void OnRequestClose(object? sender, bool saved)
    {
        Saved = saved;
        DialogResult = saved;
        Close();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is AddAccountViewModel vm && sender is PasswordBox pb)
        {
            vm.Password = pb.Password;
        }
    }
}
