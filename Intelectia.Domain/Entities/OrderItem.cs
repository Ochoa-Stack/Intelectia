using Intelectia.Domain.Common;

namespace Intelectia.Domain.Entities;

public class OrderItem : BaseEntity
{
    // Pedido al que pertenece este item
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    // Libro comprado
    public Guid BookId { get; set; }
    public Book Book { get; set; } = null!;

    // Precio capturado al momento de la compra; no cambia aunque el precio del libro cambie después (inmutable)
    public decimal PriceSnapshot { get; set; }
}
