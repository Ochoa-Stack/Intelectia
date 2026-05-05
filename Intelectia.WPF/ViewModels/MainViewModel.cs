using CommunityToolkit.Mvvm.ComponentModel;
using Intelectia.WPF.Core;
using Intelectia.WPF.Services;

namespace Intelectia.WPF.ViewModels;

public partial class MainViewModel : BaseViewModel
{
    private readonly ToastService _toastService;
    private readonly ConnectivityService _connectivityService;

    // ViewModel activo que se muestra en el ContentControl
    [ObservableProperty]
    private object? _currentViewModel;

    // Toasts activos, la MainWindow hace binding aquí
    public System.Collections.ObjectModel.ObservableCollection<ToastMessage> Toasts
        => _toastService.Toasts;

    // Banner de sin conexión
    [ObservableProperty]
    private bool _isOffline;

    public MainViewModel(ToastService toastService, ConnectivityService connectivityService)
    {
        _toastService        = toastService;
        _connectivityService = connectivityService;
        Title                = "Intelectia";

        // Sincronizamos el estado inicial
        IsOffline = !_connectivityService.IsConnected;

        // Escuchamos cambios de conectividad
        _connectivityService.ConnectivityChanged += isConnected =>
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                IsOffline = !isConnected;
                if (!isConnected)
                    _toastService.Warning("Sin conexión a internet.");
                else
                    _toastService.Info("Conexión restaurada.");
            });
        };
    }
}
