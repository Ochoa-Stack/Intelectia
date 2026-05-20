using Intelectia.Domain.Entities;

namespace Intelectia.Domain.Interfaces.Repositories;

public interface IRefreshTokenRepository
{
    // Trae todos los refresh tokens activos para un usuario
    Task<IReadOnlyList<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    // Busca un refresh token e incluye al usuario y sus perfiles (para renovar el token y generar JWT)
    Task<RefreshToken?> GetByTokenWithUserProfilesAsync(string token, CancellationToken cancellationToken = default);

    // Busca un refresh token sin relaciones (para logout)
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    // Añade un nuevo refresh token
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);
}
