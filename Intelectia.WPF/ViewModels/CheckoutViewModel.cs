using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Intelectia.Shared.DTOs.Commerce;
using Intelectia.WPF.Core;
using Intelectia.WPF.Services;

namespace Intelectia.WPF.ViewModels;

public partial class CheckoutViewModel : BaseViewModel
{
    private readonly CommerceService _commerceService;
    private readonly NavigationService _navigationService;
    private readonly Func<OrderHistoryViewModel> _orderHistoryVmFactory;
    private readonly Func<CartViewModel> _cartVmFactory;

    // Items del carrito, pasados desde CartViewModel, sin segunda llamada a la API
    public ObservableCollection<CartItemDto> Items { get; } = new();

    [ObservableProperty]
    private decimal _total;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _successMessage = string.Empty;

    [ObservableProperty]
    private bool _orderConfirmed;

    [ObservableProperty]
    private Guid _confirmedOrderId;

    public bool HasError   => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasSuccess => !string.IsNullOrEmpty(SuccessMessage);

    public CheckoutViewModel(
        CommerceService commerceService,
        NavigationService navigationService,
        Func<OrderHistoryViewModel> orderHistoryVmFactory,
        Func<CartViewModel> cartVmFactory)
    {
        _commerceService       = commerceService;
        _navigationService     = navigationService;
        _orderHistoryVmFactory = orderHistoryVmFactory;
        _cartVmFactory         = cartVmFactory;
        Title = "Checkout";
    }

    // Recibe los items del carrito antes de mostrar la vista
    public Task LoadAsync(List<CartItemDto> items, decimal total)
    {
        Items.Clear();
        foreach (var item in items)
            Items.Add(item);

        Total = total;
        return Task.CompletedTask;
    }

    // Confirma el pedido; crea el PaymentIntent en Stripe vía la API
    [RelayCommand]
    private async Task ConfirmOrderAsync()
    {
        IsBusy = true;
        ErrorMessage = string.Empty;

        try
        {
            var response = await _commerceService.CreateOrderAsync();

            ConfirmedOrderId = response.OrderId;
            OrderConfirmed   = true;

            // El webhook de Stripe confirma el pago automáticamente en Sandbox
            // En producción el cliente usaría el ClientSecret con el SDK de Stripe
            var shortId = response.OrderId.ToString()[..8].ToUpper();
            SuccessMessage =
                $"Pedido #{shortId} creado exitosamente. " +
                $"Total: ${response.Total:F2}. " +
                $"Tu biblioteca se actualizará al confirmar el pago.";

            OnPropertyChanged(nameof(HasSuccess));
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
            OnPropertyChanged(nameof(HasError));
        }
        catch
        {
            ErrorMessage = "No se pudo procesar el pedido. Intenta de nuevo.";
            OnPropertyChanged(nameof(HasError));
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Navega al historial de pedidos después de confirmar
    [RelayCommand]
    private async Task ViewOrdersAsync()
    {
        var vm = _orderHistoryVmFactory();
        await vm.LoadAsync();
        _navigationService.NavigateTo(vm);
    }

    // Vuelve al carrito si el usuario no confirmó aún
    [RelayCommand]
    private async Task GoBackAsync()
    {
        var vm = _cartVmFactory();
        await vm.LoadAsync();
        _navigationService.NavigateTo(vm);
    }
}
