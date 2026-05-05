using System.Diagnostics;
using System.Net;
using System.Text;
using System.Web;
using Intelectia.Shared.DTOs.Auth;

namespace Intelectia.WPF.Services;

public class GoogleAuthService
{
    private readonly ApiClient _apiClient;
    private readonly TokenStore _tokenStore;
    private readonly CredentialService _credentialService;

    // Puerto local para capturar el callback de Google
    private const int CallbackPort = 5100;
    private static readonly string RedirectUri = $"http://localhost:{CallbackPort}/auth/callback";

    public GoogleAuthService(
        ApiClient apiClient,
        TokenStore tokenStore,
        CredentialService credentialService)
    {
        _apiClient         = apiClient;
        _tokenStore        = tokenStore;
        _credentialService = credentialService;
    }

    // Inicia el flujo OAuth abriendo el navegador y esperando el callback
    public async Task<AuthResponseDto> AuthenticateAsync(CancellationToken cancellationToken = default)
    {
        // Generamos un state aleatorio para prevenir CSRF
        var state = Guid.NewGuid().ToString("N");

        // Pedimos al backend la URL de autorización de Google
        var authUrlResponse = await _apiClient.GetAsync<GoogleAuthUrlDto>(
            $"api/auth/google/url?redirectUri={Uri.EscapeDataString(RedirectUri)}&state={state}",
            cancellationToken);

        // Abrimos el navegador del sistema con la URL de Google
        Process.Start(new ProcessStartInfo
        {
            FileName        = authUrlResponse.AuthUrl,
            UseShellExecute = true
        });

        // Escuchamos el callback de Google en localhost
        var code = await ListenForCallbackAsync(state, cancellationToken);

        // Enviamos el code al backend para canjear por tokens
        var response = await _apiClient.PostAsync<AuthResponseDto>(
            "api/auth/google/callback",
            new GoogleCallbackRequest
            {
                Code        = code,
                RedirectUri = RedirectUri
            },
            cancellationToken);

        // Guardamos la sesión
        _tokenStore.AccessToken = response.AccessToken;
        _credentialService.SaveRefreshToken(response.RefreshToken);

        return response;
    }

    // Levanta un listener HTTP temporal para capturar el código de autorización
    private static async Task<string> ListenForCallbackAsync(
        string expectedState, CancellationToken cancellationToken)
    {
        using var listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{CallbackPort}/auth/callback/");
        listener.Start();

        // Esperamos la petición con timeout de 5 minutos
        var contextTask = listener.GetContextAsync();
        var timeoutTask = Task.Delay(TimeSpan.FromMinutes(5), cancellationToken);

        var completedTask = await Task.WhenAny(contextTask, timeoutTask);

        if (completedTask == timeoutTask)
        {
            listener.Stop();
            throw new TimeoutException("El tiempo de autenticación con Google expiró.");
        }

        var context  = await contextTask;
        var query    = context.Request.QueryString;
        var code     = query["code"]  ?? throw new InvalidOperationException("No se recibió código de Google.");
        var state    = query["state"] ?? string.Empty;

        // Respondemos al navegador con una página de cierre
        var responseHtml = Encoding.UTF8.GetBytes(
            "<html><body style='font-family:sans-serif;text-align:center;padding:40px'>" +
            "<h2>✅ Autenticación completada</h2>" +
            "<p>Puedes cerrar esta ventana y volver a Intelectia.</p>" +
            "</body></html>");

        context.Response.ContentType     = "text/html";
        context.Response.ContentLength64 = responseHtml.Length;
        await context.Response.OutputStream.WriteAsync(responseHtml, cancellationToken);
        context.Response.OutputStream.Close();

        listener.Stop();

        // Verificamos el state para prevenir CSRF
        if (state != expectedState)
            throw new InvalidOperationException("State de OAuth inválido.");

        return code;
    }
}

// DTO para la URL de autorización de Google
public class GoogleAuthUrlDto
{
    public string AuthUrl { get; set; } = string.Empty;
}

// DTO para el callback de Google
public class GoogleCallbackRequest
{
    public string Code { get; set; } = string.Empty;
    public string RedirectUri { get; set; } = string.Empty;
}
