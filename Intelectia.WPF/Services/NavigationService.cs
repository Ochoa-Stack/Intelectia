using System.Windows.Controls;
using Intelectia.WPF.ViewModels;

namespace Intelectia.WPF.Services;

public class NavigationService
{
    private MainViewModel? _mainViewModel;

    // App.xaml.cs llama esto después de crear MainViewModel
    public void Initialize(MainViewModel mainViewModel)
        => _mainViewModel = mainViewModel;

    // Cambia el ViewModel activo; el DataTemplate en App.xaml decide qué View mostrar
    public void NavigateTo<TViewModel>(TViewModel viewModel) where TViewModel : class
    {
        if (_mainViewModel is null)
            throw new InvalidOperationException("NavigationService no fue inicializado.");

        _mainViewModel.CurrentViewModel = viewModel;
    }

    public void GoBack()
    {
        // Se implementa con historial en Fase 8 si se requiere
    }
}
