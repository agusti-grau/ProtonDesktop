using ProtonDesktop.Core.Interfaces;

namespace ProtonDesktop.Services.Navigation;

public class NavigationService : INavigationService
{
    public event EventHandler<NavigatedEventArgs>? Navigated;
    public string CurrentView { get; private set; } = string.Empty;

    public void NavigateTo<TViewModel>() where TViewModel : class
    {
        CurrentView = typeof(TViewModel).Name;
        Navigated?.Invoke(this, new NavigatedEventArgs { ViewName = CurrentView });
    }

    public void NavigateTo<TViewModel>(object parameter) where TViewModel : class
    {
        CurrentView = typeof(TViewModel).Name;
        Navigated?.Invoke(this, new NavigatedEventArgs { ViewName = CurrentView, Parameter = parameter });
    }
}
