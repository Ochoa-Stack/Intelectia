using Microsoft.EntityFrameworkCore;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Infrastructure.Persistence;

namespace Intelectia.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    // Busca por email en minúsculas para evitar duplicados por capitalización
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted, cancellationToken);

    // Busca por el ID que Google devuelve al autenticar
    public Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default)
        => _context.Users
            .FirstOrDefaultAsync(u => u.GoogleId == googleId && !u.IsDeleted, cancellationToken);

    // Carga el usuario con sus perfiles y tokens para operaciones de auth
    public Task<User?> GetByIdWithProfilesAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Users
            .Include(u => u.StudentProfile)
            .Include(u => u.VendorProfile)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);

    // Agrega el usuario nuevo al contexto para que EF lo inserte
    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        => await _context.Users.AddAsync(user, cancellationToken);

    // Marca el usuario como modificado para que EF genere el UPDATE
    public void Update(User user)
        => _context.Users.Update(user);

    // Verifica si el email ya está registrado sin traer el objeto completo
    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        => _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted, cancellationToken);
}
