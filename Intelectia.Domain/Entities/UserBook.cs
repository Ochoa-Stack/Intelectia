using Intelectia.Domain.Common;

namespace Intelectia.Domain.Entities;

public class UserBook : BaseEntity
{
    // Usuario dueño del libro
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Libro adquirido
    public Guid BookId { get; set; }
    public Book Book { get; set; } = null!;

    // Fecha en que fue adquirido
    public DateTime AcquiredAt { get; set; } = DateTime.UtcNow;

    // Última página leída; para retomar la lectura
    public int LastPageRead { get; set; } = 0;

    // Porcentaje de progreso de lectura (0.0 – 100.0)
    public double ReadingProgress { get; set; } = 0;
}
