using Intelectia.Domain.Common;

namespace Intelectia.Domain.Entities;

public class Cart : BaseEntity
{
    // Dueño del carrito; uno por usuario
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Items que el usuario agregó al carrito
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();

    // Total calculado del carrito; se resuelve en memoria con los precios de los items
    public decimal Total => Items.Sum(i => i.PriceSnapshot);
}
