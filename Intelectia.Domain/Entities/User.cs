using Intelectia.Domain.Common;
using Intelectia.Domain.Enums;

namespace Intelectia.Domain.Entities;

public class User : BaseEntity
{
    // Datos de identificación básica
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    // URL de la foto de perfil almacenada en Azure Blob
    public string? ProfilePictureUrl { get; set; }

    // Contraseña hasheada; null si el usuario entró con Google
    public string? PasswordHash { get; set; }

    // Indica cómo se registró el usuario
    public AuthProvider AuthProvider { get; set; } = AuthProvider.Local;

    // ID que devuelve Google cuando el usuario se autentica con esa cuenta
    public string? GoogleId { get; set; }

    // Indica si el correo fue verificado
    public bool EmailConfirmed { get; set; } = false;

    // Token temporal para restablecer contraseña
    public string? PasswordResetToken { get; set; }

    // Fecha límite para usar el token de restablecimiento
    public DateTime? PasswordResetTokenExpiry { get; set; }

    // Fecha del último acceso al sistema
    public DateTime? LastLoginAt { get; set; }

    // Relaciones; un User puede tener ambos perfiles activos
    public StudentProfile? StudentProfile { get; set; }
    public VendorProfile? VendorProfile { get; set; }

    // Tokens de refresco activos del usuario
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    // Libros adquiridos por este usuario
    public ICollection<UserBook> UserBooks { get; set; } = new List<UserBook>();

    // Reseñas escritas por este usuario
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
