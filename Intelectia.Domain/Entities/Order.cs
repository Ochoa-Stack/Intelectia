using Intelectia.Domain.Common;
using Intelectia.Domain.Enums;

namespace Intelectia.Domain.Entities;

public class Order : BaseEntity
{
    // Usuario que realizó el pedido
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Estado actual del pedido
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    // Total cobrado al usuario; capturado al momento del checkout
    public decimal Total { get; set; }

    // Items incluidos en este pedido
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    // Pago asociado al pedido; null hasta que se inicia el proceso de pago
    public Payment? Payment { get; set; }

    // Fecha en que el pedido fue pagado
    public DateTime? PaidAt { get; set; }

    // Fecha en que el pedido fue cancelado
    public DateTime? CancelledAt { get; set; }
}
