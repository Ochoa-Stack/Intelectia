using System.Net.NetworkInformation;

namespace Intelectia.WPF.Services;

public class ConnectivityService
{
    // Evento que se dispara cuando cambia el estado de conexión
    public event Action<bool>? ConnectivityChanged;

    public bool IsConnected { get; private set; } = true;

    public ConnectivityService()
    {
        // Escuchamos cambios de red del sistema operativo
        NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
        IsConnected = NetworkInterface.GetIsNetworkAvailable();
    }

    private void OnNetworkAvailabilityChanged(
        object? sender, NetworkAvailabilityEventArgs e)
    {
        IsConnected = e.IsAvailable;
        ConnectivityChanged?.Invoke(IsConnected);
    }
}
