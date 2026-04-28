using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Intelectia.Application.Common.Interfaces;
using Intelectia.Domain.Interfaces;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Infrastructure.Persistence;
using Intelectia.Infrastructure.Repositories;
using Intelectia.Infrastructure.Services;

namespace Intelectia.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("Default"),
                sql => sql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(
            p => p.GetRequiredService<AppDbContext>());

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        // Registramos el repositorio concreto de usuarios
        services.AddScoped<IUserRepository, UserRepository>();

        // Registramos los servicios de infraestructura
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IEmailService, EmailService>();

        // Registramos el seeder como servicio con acceso al contexto
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
