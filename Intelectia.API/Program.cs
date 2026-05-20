using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;
using Intelectia.Application;
using Intelectia.Infrastructure;
using Intelectia.Infrastructure.Persistence;
using Intelectia.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Registro de capas de la aplicación
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHttpClient();

// Configuración de autenticación JWT y validación estricta del token
// Obtenemos el secreto JWT (soporta user-secrets y variables de entorno como JwtSettings__Secret)
var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? throw new InvalidOperationException("JwtSettings:Secret no está configurado en user-secrets o variables de entorno.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience            = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        // Sin tolerancia de tiempo, el token expira exactamente cuando dice
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Registramos SignalR para el chat en tiempo real
builder.Services.AddSignalR();

// Servicios base para controladores y exploración de endpoints (API)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger con soporte para enviar el token JWT desde la UI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo { Title = "Intelectia API", Version = "v1" });

    // Definimos el esquema de seguridad Bearer
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Description = "JWT Authorization. Escribe: Bearer {tu_token}",
        Name        = "Authorization",
        In          = Microsoft.OpenApi.ParameterLocation.Header,
        Type        = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme      = "bearer",
        BearerFormat = "JWT"
    });

    // Aplicamos el esquema a todos los endpoints -> OpenApi 2.x usa OpenApiSecuritySchemeReference
    c.AddSecurityRequirement(doc => new Microsoft.OpenApi.OpenApiSecurityRequirement
    {{
        new Microsoft.OpenApi.OpenApiSecuritySchemeReference("Bearer", doc),
        new List<string>()
    }});
});

builder.Services.AddHealthChecks();

// Leemos los orígenes permitidos (soporta variables de entorno como AllowedOrigins__0)
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();
if (allowedOrigins == null || allowedOrigins.Length == 0)
{
    allowedOrigins = new[] { "*" };
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", p =>
    {
        if (allowedOrigins.Contains("*"))
        {
            // Permitimos todo solo si está configurado explícitamente (Desarrollo)
            p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
        else
        {
            // Restringimos a orígenes específicos y permitimos credenciales (Producción)
            p.WithOrigins(allowedOrigins)
             .AllowAnyMethod()
             .AllowAnyHeader()
             .AllowCredentials();
        }
    });
});

// Configuramos límites de peticiones para prevenir fuerza bruta
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("AuthPolicy", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5; // Máximo 5 intentos por minuto
        opt.QueueLimit = 0;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Construye la instancia de la aplicación web configurada (Pipeline)
var app = builder.Build();

// Corremos el seeder solo en entorno de desarrollo
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    
    // Implementamos un mecanismo de reintento simple para dar tiempo a que PostgreSQL levante en Docker
    int maxRetries = 5;
    for (int retry = 1; retry <= maxRetries; retry++)
    {
        try
        {
            await seeder.SeedAsync();
            break;
        }
        catch (Exception ex) when (retry < maxRetries)
        {
            app.Logger.LogWarning(ex, "Error al inicializar la base de datos. Reintentando en 5 segundos...");
            await Task.Delay(5000);
        }
    }
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("DefaultPolicy");
app.UseRateLimiter();
app.UseHttpsRedirection();

// Dado que el orden importa; se ejecuta primero autenticación, luego autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Mapeamos el Hub de chat
app.MapHub<Intelectia.API.Hubs.ChatHub>("/hubs/chat");

app.Run();
