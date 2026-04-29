using CommunityToolkit.Mvvm.Input;
using Intelectia.WPF.Core;
using Intelectia.WPF.Services;

namespace Intelectia.WPF.ViewModels.Auth;

public partial class ForgotPasswordViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;
    private readonly Func<LoginViewModel> _loginVmFactory;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private string _email = string.Empty;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private string _errorMessage = string.Empty;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private string _successMessage = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasSuccess => !string.IsNullOrEmpty(SuccessMessage);

    public ForgotPasswordViewModel(
        AuthService authService,
        NavigationService navigationService,
        Func<LoginViewModel> loginVmFactory)
    {
        _authService = authService;
        _navigationService = navigationService;
        _loginVmFactory = loginVmFactory;
        Title = "Recuperar contraseña";
    }

    partial void OnErrorMessageChanged(string value)
        => OnPropertyChanged(nameof(HasError));

    partial void OnSuccessMessageChanged(string value)
        => OnPropertyChanged(nameof(HasSuccess));

    // Envía la solicitud de recuperación al servidor
    [RelayCommand]
    private async Task SendResetEmailAsync()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
        IsBusy = true;

        try
        {
            await _authService.ForgotPasswordAsync(Email);
            SuccessMessage = "Si el correo está registrado, recibirás instrucciones en breve.";
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch
        {
            ErrorMessage = "No se pudo conectar con el servidor.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Vuelve a la pantalla de login
    [RelayCommand]
    private void GoToLogin()
        => _navigationService.NavigateTo(_loginVmFactory());
}
