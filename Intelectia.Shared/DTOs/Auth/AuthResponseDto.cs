namespace Intelectia.Shared.DTOs.Auth;

public class AuthResponseDto
{
    // Token JWT para autenticar las peticiones
    public string AccessToken { get; set; } = string.Empty;

    // Token de larga duración para renovar el JWT sin volver a loguearse
    public string RefreshToken { get; set; } = string.Empty;

    // Fecha exacta en que expira el access token
    public DateTime AccessTokenExpiry { get; set; }

    // Datos básicos del usuario para mostrar en la UI
    public UserDto User { get; set; } = null!;
}
