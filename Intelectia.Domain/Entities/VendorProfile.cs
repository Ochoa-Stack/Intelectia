using Intelectia.Domain.Common;

namespace Intelectia.Domain.Entities;

public class VendorProfile : BaseEntity
{
    // Usuario al que pertenece este perfil
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Nombre comercial del vendedor
    public string BusinessName { get; set; } = string.Empty;

    // Descripción pública del vendedor
    public string? Description { get; set; }

    // ID de cuenta en Stripe para recibir pagos
    public string? StripeAccountId { get; set; }

    // Indica si el vendedor completó el proceso de activación
    public bool IsActive { get; set; } = false;

    // Fecha en que el perfil de vendedor fue aprobado
    public DateTime? ActivatedAt { get; set; }

    // Libros publicados por este vendedor
    public ICollection<Book> Books { get; set; } = new List<Book>();
}
