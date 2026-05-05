using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Intelectia.WPF.Services;

public enum ToastType { Success, Error, Warning, Info }

public class ToastMessage
{
    public string Message { get; set; } = string.Empty;
    public ToastType Type { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

// Servicio Singleton que gestiona la cola de notificaciones
public class ToastService
{
    // Lista observable de toasts activos, la UI hace binding aquí
    public ObservableCollection<ToastMessage> Toasts { get; } = new();

    // Muestra un toast y lo elimina automáticamente después de 4 segundos
    public void Show(string message, ToastType type = ToastType.Info)
    {
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            var toast = new ToastMessage { Message = message, Type = type };
            Toasts.Add(toast);

            // Removemos el toast después de 4 segundos sin bloquear el hilo
            _ = Task.Delay(4000).ContinueWith(_ =>
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    Toasts.Remove(toast)));
        });
    }

    public void Success(string message) => Show(message, ToastType.Success);
    public void Error(string message)   => Show(message, ToastType.Error);
    public void Warning(string message) => Show(message, ToastType.Warning);
    public void Info(string message)    => Show(message, ToastType.Info);
}
