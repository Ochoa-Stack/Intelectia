using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Intelectia.Shared.DTOs.Commerce;
using Intelectia.WPF.Core;
using Intelectia.WPF.Services;

namespace Intelectia.WPF.ViewModels;

public partial class CartViewModel : BaseViewModel
{
    private readonly CommerceService _commerceService;
    private readonly NavigationService _navigationService;
    private readonly Func<CheckoutViewModel> _checkoutVmFactory;
    private readonly Func<MarketplaceViewModel> _marketplaceVmFactory;

    public ObservableCollection<CartItemDto> Items { get; } = new();

    [ObservableProperty]
    private decimal _total;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool IsEmpty  => Items.Count == 0;

    public CartViewModel(
        CommerceService commerceService,
        NavigationService navigationService,
        Func<CheckoutViewModel> checkoutVmFactory,
        Func<MarketplaceViewModel> marketplaceVmFactory)
    {
        _commerceService      = commerceService;
        _navigationService    = navigationService;
        _checkoutVmFactory    = checkoutVmFactory;
        _marketplaceVmFactory = marketplaceVmFactory;
        Title = "Mi carrito";
    }

    // Carga el carrito desde la API
    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var cart = await _commerceService.GetCartAsync();
            SyncCart(cart);
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
            OnPropertyChanged(nameof(HasError));
        }
        catch
        {
            ErrorMessage = "No se pudo cargar el carrito.";
            OnPropertyChanged(nameof(HasError));
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Elimina un item del carrito
    [RelayCommand]
    private async Task RemoveItemAsync(CartItemDto item)
    {
        IsBusy = true;

        try
        {
            var cart = await _commerceService.RemoveFromCartAsync(item.Id);
            SyncCart(cart);
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

    // Navega al checkout con los items actuales del carrito
    [RelayCommand]
    private async Task ProceedToCheckoutAsync()
    {
        var vm = _checkoutVmFactory();
        await vm.LoadAsync(Items.ToList(), Total);
        _navigationService.NavigateTo(vm);
    }

    // Vuelve al catálogo
    [RelayCommand]
    private async Task ContinueShoppingAsync()
    {
        var vm = _marketplaceVmFactory();
        await vm.InitializeAsync();
        _navigationService.NavigateTo(vm);
    }

    // Sincroniza la colección observable con el DTO recibido de la API
    private void SyncCart(CartDto cart)
    {
        Items.Clear();
        foreach (var item in cart.Items)
            Items.Add(item);

        Total = cart.Total;
        OnPropertyChanged(nameof(IsEmpty));
    }
}
