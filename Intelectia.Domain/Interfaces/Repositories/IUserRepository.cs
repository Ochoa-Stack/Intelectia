using Intelectia.Domain.Entities;

namespace Intelectia.Domain.Interfaces.Repositories;

public interface IUserRepository
{
    // Busca un usuario por su dirección de correo (sin navigation properties)
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    // Busca por email e incluye perfiles y tokens en una sola consulta
    Task<User?> GetByEmailWithProfilesAsync(string email, CancellationToken cancellationToken = default);

    // Busca un usuario por su ID de cuenta Google
    Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default);

    // Busca un usuario por ID e incluye sus perfiles y tokens
    Task<User?> GetByIdWithProfilesAsync(Guid id, CancellationToken cancellationToken = default);

    // Registra un nuevo usuario en la base de datos
    Task AddAsync(User user, CancellationToken cancellationToken = default);

    // Marca el usuario como modificado para que EF lo persista
    void Update(User user);

    // Verifica si ya existe un correo registrado
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}
