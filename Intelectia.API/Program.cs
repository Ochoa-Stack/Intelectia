using Intelectia.Application;
using Intelectia.Infrastructure;
using Intelectia.Infrastructure.Persistence;
using Intelectia.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Registramos las capas de la aplicación
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Registramos los servicios de la API
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Intelectia API", Version = "v1" });
    // La configuración Bearer de Swagger se agrega en Fase 3 junto con JWT
});

// Registramos el health check
builder.Services.AddHealthChecks();

// Política de CORS abierta solo para desarrollo
builder.Services.AddCors(options =>
    options.AddPolicy("DevPolicy", p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// Corremos el seeder solo en entorno de desarrollo
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
}

// Middleware global de manejo de errores
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("DevPolicy");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
