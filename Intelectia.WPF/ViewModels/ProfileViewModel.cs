using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Intelectia.Shared.DTOs.Profile;
using Intelectia.WPF.Core;
using Intelectia.WPF.Services;
using Intelectia.WPF.ViewModels.Auth;

namespace Intelectia.WPF.ViewModels;

public partial class ProfileViewModel : BaseViewModel
{
    private readonly ProfileService _profileService;
    private readonly NavigationService _navigationService;
    private readonly Func<MarketplaceViewModel> _marketplaceVmFactory;
    private readonly AuthService _authService;
    private readonly Func<LoginViewModel> _loginVmFactory;

    [ObservableProperty]
    private string _firstName = string.Empty;

    [ObservableProperty]
    private string _lastName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private bool _isStudent;

    [ObservableProperty]
    private bool _isVendor;

    [ObservableProperty]
    private string _activeTab = "profile";

    // Campos para cambio de contraseña
    [ObservableProperty]
    private string _currentPassword = string.Empty;

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _confirmNewPassword = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _successMessage = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasSuccess => !string.IsNullOrEmpty(SuccessMessage);
    public bool IsProfileTab => ActiveTab == "profile";
    public bool IsSecurityTab => ActiveTab == "security";

    public ProfileViewModel(
        ProfileService profileService,
        NavigationService navigationService,
        Func<MarketplaceViewModel> marketplaceVmFactory,
        AuthService authService,
        Func<LoginViewModel> loginVmFactory)
    {
        _profileService       = profileService;
        _navigationService    = navigationService;
        _marketplaceVmFactory = marketplaceVmFactory;
        _authService          = authService;
        _loginVmFactory       = loginVmFactory;
        Title = "Mi Perfil";
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        IsBusy = true;

        try
        {
            var profile = await _profileService.GetProfileAsync();
            FirstName = profile.FirstName;
            LastName  = profile.LastName;
            Email     = profile.Email;
            IsStudent = profile.IsStudent;
            IsVendor  = profile.IsVendor;
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
            OnPropertyChanged(nameof(HasError));
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Guarda los datos del perfil
    [RelayCommand]
    private async Task SaveProfileAsync()
    {
        ErrorMessage  = string.Empty;
        SuccessMessage = string.Empty;
        IsBusy = true;

        try
        {
            var updated = await _profileService.UpdateProfileAsync(new UpdateProfileRequest
            {
                FirstName = FirstName,
                LastName  = LastName
            });

            FirstName     = updated.FirstName;
            LastName      = updated.LastName;
            SuccessMessage = "Perfil actualizado exitosamente.";
            OnPropertyChanged(nameof(HasSuccess));
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
            OnPropertyChanged(nameof(HasError));
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Cambia la contraseña del usuario
    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        ErrorMessage  = string.Empty;
        SuccessMessage = string.Empty;
        IsBusy = true;

        try
        {
            await _profileService.ChangePasswordAsync(new ChangePasswordRequest
            {
                CurrentPassword    = CurrentPassword,
                NewPassword        = NewPassword,
                ConfirmNewPassword = ConfirmNewPassword
            });

            CurrentPassword    = string.Empty;
            NewPassword        = string.Empty;
            ConfirmNewPassword = string.Empty;
            SuccessMessage     = "Contraseña actualizada exitosamente.";
            OnPropertyChanged(nameof(HasSuccess));
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
            OnPropertyChanged(nameof(HasError));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SwitchTabAsync(string tab)
    {
        ActiveTab      = tab;
        ErrorMessage   = string.Empty;
        SuccessMessage = string.Empty;
        OnPropertyChanged(nameof(IsProfileTab));
        OnPropertyChanged(nameof(IsSecurityTab));
        OnPropertyChanged(nameof(HasError));
        OnPropertyChanged(nameof(HasSuccess));
    }

    [RelayCommand]
    private async Task GoToMarketplaceAsync()
    {
        var vm = _marketplaceVmFactory();
        await vm.InitializeAsync();
        _navigationService.NavigateTo(vm);
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _authService.LogoutAsync();
        _navigationService.NavigateTo(_loginVmFactory());
    }
}
