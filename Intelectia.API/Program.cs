using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Intelectia.Application;
using Intelectia.Infrastructure;
using Intelectia.Infrastructure.Persistence;
using Intelectia.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Registro de capas de la aplicación
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Configuración de autenticación JWT y validación estricta del token
var jwtSecret = builder.Configuration["JwtSettings:Secret"]
    ?? throw new InvalidOperationException("JwtSettings:Secret no está configurado en user-secrets.");

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

// Política de CORS abierta solo para desarrollo
builder.Services.AddCors(options =>
    options.AddPolicy("DevPolicy", p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// Construye la instancia de la aplicación web configurada (Pipeline)
var app = builder.Build();

// Corremos el seeder solo en entorno de desarrollo
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("DevPolicy");
app.UseHttpsRedirection();

// Dado que el orden importa; se ejecuta primero autenticación, luego autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

// Mapeamos el Hub de chat
app.MapHub<Intelectia.API.Hubs.ChatHub>("/hubs/chat");

app.Run();
