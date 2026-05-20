using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Intelectia.Application.Common.Interfaces;
using Intelectia.Domain.Interfaces;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Infrastructure.Persistence;
using Intelectia.Infrastructure.Persistence.Repositories;
using Intelectia.Infrastructure.Repositories;
using Intelectia.Infrastructure.Services;

namespace Intelectia.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Conectamos EF Core con PostgreSQL usando la connection string
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("Default"),
                npgsql => npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // Registramos el patrón Unit of Work y los repositorios genéricos
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IVendorRepository, VendorRepository>();
        services.AddScoped<IUserBookRepository, UserBookRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();

        // Registramos los repositorios concretos de auth
        services.AddScoped<IUserRepository, UserRepository>();

        // Registramos los repositorios del Marketplace
        services.AddScoped<IBookRepository, BookRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();

        // Registramos los repositorios de Comercio
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Registramos los repositorios de Herramientas de Estudio
        services.AddScoped<INoteRepository, NoteRepository>();
        services.AddScoped<ICitationRepository, CitationRepository>();

        // Repositorios de grupos
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IGroupMessageRepository, GroupMessageRepository>();

        // Registramos los servicios de infraestructura
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<ITranslationService, DeepLTranslationService>();

        // Registramos el seeder como servicio con acceso al contexto
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
