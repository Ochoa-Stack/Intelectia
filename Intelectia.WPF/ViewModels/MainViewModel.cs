using CommunityToolkit.Mvvm.ComponentModel;
using Intelectia.WPF.Core;

namespace Intelectia.WPF.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    // El ViewModel que está visible en este momento en la ventana
    [ObservableProperty]
    private object? _currentViewModel;

    public MainViewModel()
    {
        Title = "Intelectia";
    }
}
