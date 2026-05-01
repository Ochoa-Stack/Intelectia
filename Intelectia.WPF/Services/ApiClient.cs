using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;

namespace Intelectia.WPF.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;

    // Opciones de serialización; nombres de propiedades en camelCase como los devuelve la API
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // Hace un POST y deserializa la respuesta al tipo pedido
    public async Task<T> PostAsync<T>(string endpoint, object body, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(endpoint, body, cancellationToken);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("La respuesta de la API estaba vacía.");
    }

    // Hace un POST que no devuelve cuerpo (como logout)
    public async Task PostAsync(string endpoint, object body, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync(endpoint, body, cancellationToken);
        await EnsureSuccessAsync(response);
    }

    // Hace un GET y deserializa la respuesta al tipo pedido
    public async Task<T> GetAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(endpoint, cancellationToken);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("La respuesta de la API estaba vacía.");
    }

    // Agrega el JWT a todas las peticiones que lo requieran
    public void SetAuthorizationToken(string token)
        => _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

    // Limpia el token al hacer logout
    public void ClearAuthorizationToken()
        => _httpClient.DefaultRequestHeaders.Authorization = null;

    // Lee el mensaje de error que devuelve la API y lo lanza como excepción
    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var content = await response.Content.ReadAsStringAsync();

        try
        {
            // Intentamos leer el mensaje estructurado que devuelve GlobalExceptionMiddleware
            using var doc = JsonDocument.Parse(content);
            var message = doc.RootElement
                .GetProperty("message")
                .GetString() ?? "Error desconocido.";
            throw new ApiException(message, (int)response.StatusCode);
        }
        catch (JsonException)
        {
            // Si no tiene el formato esperado, lanzamos el contenido crudo
            throw new ApiException(content, (int)response.StatusCode);
        }
    }
}

// Excepción que representa un error con código HTTP de la API
public class ApiException : Exception
{
    public int StatusCode { get; }

    public ApiException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}
