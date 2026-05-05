using Intelectia.Domain.Common;

namespace Intelectia.Domain.Entities;

public class RefreshToken : BaseEntity
{
    // Usuario dueño de este token
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // El token en sí; se almacena hasheado
    public string Token { get; set; } = string.Empty;

    // Fecha en que deja de ser válido
    public DateTime ExpiresAt { get; set; }

    // Fecha en que fue usado para generar un nuevo par de tokens
    public DateTime? UsedAt { get; set; }

    // Fecha en que fue revocado manualmente (logout)
    public DateTime? RevokedAt { get; set; }

    // Indica si sigue siendo válido; propiedad calculada, sin setter ni columna en BD
    public bool IsActive => RevokedAt is null && UsedAt is null && DateTime.UtcNow < ExpiresAt;
}
