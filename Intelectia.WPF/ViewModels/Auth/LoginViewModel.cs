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
    private readonly ToastService _toastService;
    private readonly GoogleAuthService _googleAuthService;
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
        ToastService toastService,
        Func<GoogleAuthService> googleAuthServiceFactory,
        Func<RegisterViewModel> registerVmFactory,
        Func<ForgotPasswordViewModel> forgotPasswordVmFactory,
        Func<MarketplaceViewModel> marketplaceVmFactory)
    {
        _authService             = authService;
        _navigationService       = navigationService;
        _toastService            = toastService;
        _googleAuthService       = googleAuthServiceFactory();
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
            _toastService.Error(ex.Message);
        }
        catch
        {
            var msg = "No se pudo conectar con el servidor. Verifica tu conexión.";
            ErrorMessage = msg;
            _toastService.Error(msg);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoginWithGoogleAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var session = await _googleAuthService.AuthenticateAsync();

            // Guardamos la sesión manualmente ya que GoogleAuthService la construye
            _authService.SetSessionFromExternal(session);

            var marketplaceVm = _marketplaceVmFactory();
            await marketplaceVm.InitializeAsync();
            _navigationService.NavigateTo(marketplaceVm);
        }
        catch (TimeoutException)
        {
            ErrorMessage = "La autenticación con Google expiró. Intenta de nuevo.";
            _toastService.Warning(ErrorMessage);
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
            _toastService.Error(ex.Message);
        }
        catch
        {
            ErrorMessage = "No se pudo completar la autenticación con Google.";
            _toastService.Error(ErrorMessage);
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
