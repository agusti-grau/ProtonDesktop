using System.Windows;
using ProtonDesktop.ViewModels.Settings;

namespace ProtonDesktop.Views.Settings;

public partial class SettingsView : Window
{
    public SettingsView()
    {
        InitializeComponent();
        var viewModel = App.Services.GetService(typeof(SettingsViewModel)) as SettingsViewModel
            ?? new SettingsViewModel(
                App.Services.GetService(typeof(ProtonDesktop.Core.Interfaces.IAccountRepository)) as ProtonDesktop.Core.Interfaces.IAccountRepository,
                App.Services.GetService(typeof(ProtonDesktop.Core.Interfaces.ICredentialStore)) as ProtonDesktop.Core.Interfaces.ICredentialStore,
                App.Services.GetService(typeof(Serilog.ILogger)) as Serilog.ILogger);
        DataContext = viewModel;
        Loaded += SettingsView_Loaded;
    }

    private async void SettingsView_Loaded(object sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as SettingsViewModel;
        if (viewModel != null)
        {
            await viewModel.LoadCommand.ExecuteAsync(null);
        }
    }

    private async void AddAccount_Click(object sender, RoutedEventArgs e)
    {
        var addAccountVm = App.Services.GetService(typeof(AddAccountViewModel)) as AddAccountViewModel;
        if (addAccountVm == null) return;

        var dialog = new AddAccountDialog(addAccountVm) { Owner = this };
        dialog.ShowDialog();

        if (dialog.Saved && DataContext is SettingsViewModel vm)
        {
            await vm.LoadCommand.ExecuteAsync(null);
        }
    }
}
