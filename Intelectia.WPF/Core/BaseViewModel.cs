using CommunityToolkit.Mvvm.ComponentModel;

namespace Intelectia.WPF.Core;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _title = string.Empty;
}
