using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Intelectia.Infrastructure.Persistence;

public class DatabaseSeeder
{
    private readonly AppDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(AppDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Punto de entrada principal del seeder
    public async Task SeedAsync()
    {
        // Aplicamos migraciones pendientes si las hay
        await _context.Database.MigrateAsync();

        _logger.LogInformation("Iniciando proceso de seed...");

        // Aquí se llaman los métodos de seed por entidad
        // Se van agregando conforme avanzan las fases

        _logger.LogInformation("Seed completado.");
    }
}
