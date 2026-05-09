using CommunityToolkit.Mvvm.Input;
using Intelectia.Shared.DTOs.Auth;
using Intelectia.WPF.Core;
using Intelectia.WPF.Services;

namespace Intelectia.WPF.ViewModels.Auth;

public partial class RegisterViewModel : BaseViewModel
{
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;
    private readonly Func<LoginViewModel> _loginVmFactory;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private string _firstName = string.Empty;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private string _lastName = string.Empty;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private string _email = string.Empty;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private string _password = string.Empty;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private string _confirmPassword = string.Empty;

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private string _errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public RegisterViewModel(
        AuthService authService,
        NavigationService navigationService,
        Func<LoginViewModel> loginVmFactory)
    {
        _authService = authService;
        _navigationService = navigationService;
        _loginVmFactory = loginVmFactory;
        Title = "Crear cuenta";
    }

    partial void OnErrorMessageChanged(string value)
        => OnPropertyChanged(nameof(HasError));

    // Comando que se ejecuta al presionar el botón de registro
    [RelayCommand]
    private async Task RegisterAsync()
    {
        ErrorMessage = string.Empty;

        // Validación básica en cliente para feedback inmediato
        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName))
        {
            ErrorMessage = "El nombre y apellido son obligatorios.";
            OnPropertyChanged(nameof(HasError));
            return;
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            ErrorMessage = "El correo electrónico es obligatorio.";
            OnPropertyChanged(nameof(HasError));
            return;
        }

        if (string.IsNullOrWhiteSpace(Password) || Password.Length < 8)
        {
            ErrorMessage = "La contraseña debe tener al menos 8 caracteres.";
            OnPropertyChanged(nameof(HasError));
            return;
        }

        if (Password != ConfirmPassword)
        {
            ErrorMessage = "Las contraseñas no coinciden.";
            OnPropertyChanged(nameof(HasError));
            return;
        }

        IsBusy = true;

        try
        {
            await _authService.RegisterAsync(new RegisterRequest
            {
                FirstName       = FirstName.Trim(),
                LastName        = LastName.Trim(),
                Email           = Email.Trim(),
                Password        = Password,
                ConfirmPassword = ConfirmPassword
            });

            // Registro exitoso; volvemos al login
            _navigationService.NavigateTo(_loginVmFactory());
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
            OnPropertyChanged(nameof(HasError));
        }
        catch
        {
            ErrorMessage = "No se pudo conectar con el servidor. Verifica tu conexión.";
            OnPropertyChanged(nameof(HasError));
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Vuelve a la pantalla de login sin registrar
    [RelayCommand]
    private void GoToLogin()
        => _navigationService.NavigateTo(_loginVmFactory());
}
