using Intelectia.Shared.DTOs.Auth;

namespace Intelectia.WPF.Services;

public class AuthService
{
    private readonly ApiClient _apiClient;

    // Guardamos la sesión en memoria mientras la app está abierta
    public AuthResponseDto? CurrentSession { get; private set; }

    // Propiedad de conveniencia para saber si hay sesión activa
    public bool IsAuthenticated => CurrentSession is not null;

    public AuthService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    // Registra un usuario nuevo y guarda la sesión recibida
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.PostAsync<AuthResponseDto>("api/auth/register", request, cancellationToken);
        SetSession(response);
        return response;
    }

    // Autentica al usuario y guarda la sesión
    public async Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.PostAsync<AuthResponseDto>("api/auth/login", request, cancellationToken);
        SetSession(response);
        return response;
    }

    // Renueva el JWT usando el refresh token guardado
    public async Task<bool> RefreshSessionAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentSession is null)
            return false;

        try
        {
            var response = await _apiClient.PostAsync<AuthResponseDto>(
                "api/auth/refresh",
                new RefreshTokenRequest { RefreshToken = CurrentSession.RefreshToken },
                cancellationToken);
            SetSession(response);
            return true;
        }
        catch
        {
            // Si el refresh falla cerramos la sesión localmente
            ClearSession();
            return false;
        }
    }

    // Revoca el refresh token en el servidor y limpia la sesión local
    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentSession is null)
            return;

        try
        {
            await _apiClient.PostAsync(
                "api/auth/logout",
                new LogoutRequest { RefreshToken = CurrentSession.RefreshToken },
                cancellationToken);
        }
        finally
        {
            // Limpiamos la sesión local sin importar si el servidor respondió bien
            ClearSession();
        }
    }

    public async Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
        => await _apiClient.PostAsync("api/auth/forgot-password", new ForgotPasswordRequest { Email = email }, cancellationToken);

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
        => await _apiClient.PostAsync("api/auth/reset-password", request, cancellationToken);

    // Guarda la sesión y pone el token en el HttpClient para peticiones futuras
    private void SetSession(AuthResponseDto session)
    {
        CurrentSession = session;
        _apiClient.SetAuthorizationToken(session.AccessToken);
    }

    // Borra la sesión y quita el token del HttpClient
    private void ClearSession()
    {
        CurrentSession = null;
        _apiClient.ClearAuthorizationToken();
    }
}
