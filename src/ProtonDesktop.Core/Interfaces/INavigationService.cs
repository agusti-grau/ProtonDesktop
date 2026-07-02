namespace ProtonDesktop.Core.Interfaces;

public interface INavigationService
{
    event EventHandler<NavigatedEventArgs>? Navigated;
    void NavigateTo<TViewModel>() where TViewModel : class;
    void NavigateTo<TViewModel>(object parameter) where TViewModel : class;
    string CurrentView { get; }
}

public class NavigatedEventArgs : EventArgs
{
    public string ViewName { get; init; } = string.Empty;
    public object? Parameter { get; init; }
}
