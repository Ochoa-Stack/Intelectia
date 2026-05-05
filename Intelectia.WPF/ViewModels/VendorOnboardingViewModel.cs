using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Intelectia.Shared.DTOs.Vendor;
using Intelectia.WPF.Core;
using Intelectia.WPF.Services;

namespace Intelectia.WPF.ViewModels;

public partial class VendorOnboardingViewModel : BaseViewModel
{
    private readonly VendorService _vendorService;
    private readonly NavigationService _navigationService;
    private readonly Func<VendorDashboardViewModel> _dashboardVmFactory;

    [ObservableProperty]
    private string _businessName = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public VendorOnboardingViewModel(
        VendorService vendorService,
        NavigationService navigationService,
        Func<VendorDashboardViewModel> dashboardVmFactory)
    {
        _vendorService     = vendorService;
        _navigationService = navigationService;
        _dashboardVmFactory = dashboardVmFactory;
        Title = "Activar perfil de vendedor";
    }

    // Activa el perfil de vendedor y navega al dashboard si tiene éxito
    [RelayCommand]
    private async Task ActivateAsync()
    {
        if (string.IsNullOrWhiteSpace(BusinessName))
        {
            ErrorMessage = "El nombre comercial es obligatorio.";
            OnPropertyChanged(nameof(HasError));
            return;
        }

        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            await _vendorService.BecomeVendorAsync(new BecomeVendorRequest
            {
                BusinessName = BusinessName,
                Description  = Description
            });

            // Activación exitosa — navegamos al dashboard del vendedor
            var vm = _dashboardVmFactory();
            await vm.InitializeAsync();
            _navigationService.NavigateTo(vm);
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
            OnPropertyChanged(nameof(HasError));
        }
        catch
        {
            ErrorMessage = "No se pudo activar el perfil. Intenta de nuevo.";
            OnPropertyChanged(nameof(HasError));
        }
        finally
        {
            IsBusy = false;
        }
    }
}
