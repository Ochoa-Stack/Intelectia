using System.Net.Http;
using System.Net.Http.Headers;

namespace Intelectia.WPF.Services;

/* DelegatingHandler que inyecta el JWT en cada petición HTTP saliente.
Resuelve el problema de que AddHttpClient<ApiClient> crea instancias distintas
de HttpClient; todas comparten el mismo TokenStore Singleton */
public class AuthTokenHandler : DelegatingHandler
{
    private readonly TokenStore _tokenStore;

    public AuthTokenHandler(TokenStore tokenStore)
    {
        _tokenStore = tokenStore;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Si hay sesión activa, inyectamos el token antes de enviar la petición
        if (_tokenStore.AccessToken is not null)
        {
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", _tokenStore.AccessToken);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
