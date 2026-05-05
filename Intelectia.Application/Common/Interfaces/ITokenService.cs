using Intelectia.Domain.Entities;

namespace Intelectia.Application.Common.Interfaces;

public interface ITokenService
{
    // Genera el JWT firmado con los datos del usuario
    string GenerateAccessToken(User user);

    // Genera un refresh token seguro y aleatorio
    string GenerateRefreshToken();

    // Devuelve cuántos minutos dura el access token
    int GetAccessTokenExpirationMinutes();
}
