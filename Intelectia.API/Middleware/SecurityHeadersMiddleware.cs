namespace Intelectia.API.Middleware;

// Añadimos cabeceras de seguridad a todas las respuestas HTTP
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Prevenimos MIME-sniffing
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        // Prevenimos Clickjacking
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        // Controlamos la información del referente
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        // Prevenimos XSS en navegadores antiguos
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        await _next(context);
    }
}
