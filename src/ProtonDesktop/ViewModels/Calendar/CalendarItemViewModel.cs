using CommunityToolkit.Mvvm.ComponentModel;
using ProtonDesktop.Core.Models;

namespace ProtonDesktop.ViewModels.Calendar;

public partial class CalendarItemViewModel : ObservableObject
{
    [ObservableProperty]
    private int _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string? _description;

    [ObservableProperty]
    private string? _color;

    [ObservableProperty]
    private bool _isVisible = true;

    public CalendarItemViewModel(ProtonDesktop.Core.Models.Calendar calendar)
    {
        Id = calendar.Id;
        Name = calendar.Name;
        Description = calendar.Description;
        Color = calendar.Color;
        IsVisible = calendar.IsVisible;
    }
}
