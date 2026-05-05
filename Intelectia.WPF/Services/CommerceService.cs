using Intelectia.Shared.DTOs.Commerce;

namespace Intelectia.WPF.Services;

public class CommerceService
{
    private readonly ApiClient _apiClient;

    public CommerceService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // Trae el carrito del usuario autenticado
    public Task<CartDto> GetCartAsync(CancellationToken cancellationToken = default)
        => _apiClient.GetAsync<CartDto>("api/cart", cancellationToken);

    // Agrega un libro al carrito
    public Task<CartDto> AddToCartAsync(Guid bookId, CancellationToken cancellationToken = default)
        => _apiClient.PostAsync<CartDto>("api/cart/items", new { BookId = bookId }, cancellationToken);

    // Elimina un item del carrito por su CartItemId
    public Task<CartDto> RemoveFromCartAsync(Guid cartItemId, CancellationToken cancellationToken = default)
        => _apiClient.DeleteAsync<CartDto>($"api/cart/items/{cartItemId}", cancellationToken);

    // Crea un pedido desde el carrito e inicia el PaymentIntent en Stripe
    public Task<CreateOrderResponse> CreateOrderAsync(CancellationToken cancellationToken = default)
        => _apiClient.PostAsync<CreateOrderResponse>("api/orders", new { }, cancellationToken);

    // Trae el historial de pedidos del usuario
    public Task<List<OrderDto>> GetOrdersAsync(CancellationToken cancellationToken = default)
        => _apiClient.GetAsync<List<OrderDto>>("api/orders", cancellationToken);

    // Trae el detalle de un pedido
    public Task<OrderDto> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        => _apiClient.GetAsync<OrderDto>($"api/orders/{orderId}", cancellationToken);

    // Cancela un pedido pendiente de pago
    public Task CancelOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
        => _apiClient.PutAsync($"api/orders/{orderId}/cancel", new { }, cancellationToken);
}
