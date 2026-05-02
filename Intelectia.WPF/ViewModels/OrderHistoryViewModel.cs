using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Intelectia.Shared.DTOs.Commerce;
using Intelectia.WPF.Core;
using Intelectia.WPF.Services;

namespace Intelectia.WPF.ViewModels;

public partial class OrderHistoryViewModel : BaseViewModel
{
    private readonly CommerceService _commerceService;
    private readonly NavigationService _navigationService;
    private readonly Func<MarketplaceViewModel> _marketplaceVmFactory;

    public ObservableCollection<OrderDto> Orders { get; } = new();

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool IsEmpty  => Orders.Count == 0;

    public OrderHistoryViewModel(
        CommerceService commerceService,
        NavigationService navigationService,
        Func<MarketplaceViewModel> marketplaceVmFactory)
    {
        _commerceService      = commerceService;
        _navigationService    = navigationService;
        _marketplaceVmFactory = marketplaceVmFactory;
        Title = "Mis pedidos";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var orders = await _commerceService.GetOrdersAsync();
            Orders.Clear();
            foreach (var order in orders)
                Orders.Add(order);

            OnPropertyChanged(nameof(IsEmpty));
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
            OnPropertyChanged(nameof(HasError));
        }
        catch
        {
            ErrorMessage = "No se pudo cargar el historial de pedidos.";
            OnPropertyChanged(nameof(HasError));
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Vuelve al catálogo
    [RelayCommand]
    private async Task GoToMarketplaceAsync()
    {
        var vm = _marketplaceVmFactory();
        await vm.InitializeAsync();
        _navigationService.NavigateTo(vm);
    }
}
