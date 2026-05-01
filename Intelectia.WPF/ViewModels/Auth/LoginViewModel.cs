using CommunityToolkit.Mvvm.Input;
using Intelectia.Shared.DTOs.Auth;
using Intelectia.WPF.Core;
using Intelectia.WPF.Services;
using Intelectia.WPF.ViewModels;

namespace Intelectia.WPF.ViewModels.Auth;

public partial class LoginViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;
    private readonly Func<RegisterViewModel> _registerVmFactory;
    private readonly Func<ForgotPasswordViewModel> _forgotPasswordVmFactory;
    private readonly Func<MarketplaceViewModel> _marketplaceVmFactory;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private string _email = string.Empty;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private string _password = string.Empty;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private string _errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public LoginViewModel(
        AuthService authService,
        NavigationService navigationService,
        Func<RegisterViewModel> registerVmFactory,
        Func<ForgotPasswordViewModel> forgotPasswordVmFactory,
        Func<MarketplaceViewModel> marketplaceVmFactory)
    {
        _authService             = authService;
        _navigationService       = navigationService;
        _registerVmFactory       = registerVmFactory;
        _forgotPasswordVmFactory = forgotPasswordVmFactory;
        _marketplaceVmFactory    = marketplaceVmFactory;
        Title = "Iniciar sesión";
    }

    // CommunityToolkit genera el setter; necesitamos notificar HasError cuando cambia ErrorMessage
    partial void OnErrorMessageChanged(string value)
        => OnPropertyChanged(nameof(HasError));

    // Comando que se ejecuta al presionar el botón de login
    [RelayCommand]
    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;
        IsBusy = true;

        try
        {
            await _authService.LoginAsync(new LoginRequest
            {
                Email    = Email,
                Password = Password
            });

            // Login exitoso; navegamos al Marketplace
            var marketplaceVm = _marketplaceVmFactory();
            await marketplaceVm.InitializeAsync();
            _navigationService.NavigateTo(marketplaceVm);
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch
        {
            ErrorMessage = "No se pudo conectar con el servidor. Verifica tu conexión.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Navega a la pantalla de registro
    [RelayCommand]
    private void GoToRegister()
        => _navigationService.NavigateTo(_registerVmFactory());

    // Navega a la pantalla de recuperación de contraseña
    [RelayCommand]
    private void GoToForgotPassword()
        => _navigationService.NavigateTo(_forgotPasswordVmFactory());
}
