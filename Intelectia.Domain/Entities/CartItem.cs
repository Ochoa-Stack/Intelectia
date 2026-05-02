using Intelectia.Domain.Common;

namespace Intelectia.Domain.Entities;

public class CartItem : BaseEntity
{
    // Carrito al que pertenece este item
    public Guid CartId { get; set; }
    public Cart Cart { get; set; } = null!;

    // Libro que el usuario quiere comprar
    public Guid BookId { get; set; }
    public Book Book { get; set; } = null!;

    // Precio capturado al momento de agregar al carrito; se usa en Cart.Total y se copia a OrderItem.PriceSnapshot al checkout
    public decimal PriceSnapshot { get; set; }
}
