using Intelectia.Shared.DTOs.Auth;

namespace Intelectia.WPF.Services;

public class AuthService
{
    private readonly ApiClient  _apiClient;
    private readonly TokenStore _tokenStore;

    // Sesión activa en memoria; null si no hay login
    public AuthResponseDto? CurrentSession { get; private set; }
    public bool IsAuthenticated => CurrentSession is not null;

    public AuthService(ApiClient apiClient, TokenStore tokenStore)
    {
        _apiClient  = apiClient;
        _tokenStore = tokenStore;
    }

    // Registra un usuario nuevo y persiste la sesión
    public async Task<AuthResponseDto> RegisterAsync(
        RegisterRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.PostAsync<AuthResponseDto>(
            "api/auth/register", request, cancellationToken);
        SetSession(response);
        return response;
    }

    // Autentica al usuario y persiste la sesión
    public async Task<AuthResponseDto> LoginAsync(
        LoginRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _apiClient.PostAsync<AuthResponseDto>(
            "api/auth/login", request, cancellationToken);
        SetSession(response);
        return response;
    }

    // Renueva el JWT usando el refresh token guardado
    public async Task<bool> RefreshSessionAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentSession is null) return false;

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
            // Refresh fallido; cerramos la sesión localmente
            ClearSession();
            return false;
        }
    }

    // Revoca el refresh token en el servidor y limpia la sesión local
    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentSession is null) return;

        try
        {
            await _apiClient.PostAsync(
                "api/auth/logout",
                new LogoutRequest { RefreshToken = CurrentSession.RefreshToken },
                cancellationToken);
        }
        finally
        {
            // Limpiamos la sesión local sin importar si el servidor respondió
            ClearSession();
        }
    }

    public async Task ForgotPasswordAsync(
        string email, CancellationToken cancellationToken = default)
        => await _apiClient.PostAsync(
            "api/auth/forgot-password",
            new ForgotPasswordRequest { Email = email },
            cancellationToken);

    public async Task ResetPasswordAsync(
        ResetPasswordRequest request, CancellationToken cancellationToken = default)
        => await _apiClient.PostAsync("api/auth/reset-password", request, cancellationToken);

    // Guarda la sesión y escribe el token en el TokenStore compartido; AuthTokenHandler lo leerá automáticamente en cada petición subsecuente
    private void SetSession(AuthResponseDto session)
    {
        CurrentSession          = session;
        _tokenStore.AccessToken = session.AccessToken;
    }

    // Borra la sesión y elimina el token del TokenStore
    private void ClearSession()
    {
        CurrentSession          = null;
        _tokenStore.AccessToken = null;
    }
}
