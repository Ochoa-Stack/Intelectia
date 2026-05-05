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

    // Busca por email sin navigation properties; para verificaciones simples como EmailExistsAsync
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted, cancellationToken);

    // Busca por email con todos los datos necesarios para auth; una sola consulta sin conflictos de tracking
    public Task<User?> GetByEmailWithProfilesAsync(string email, CancellationToken cancellationToken = default)
        => _context.Users
            .Include(u => u.StudentProfile)
            .Include(u => u.VendorProfile)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted, cancellationToken);

    public Task<User?> GetByGoogleIdAsync(string googleId, CancellationToken cancellationToken = default)
        => _context.Users
            .FirstOrDefaultAsync(u => u.GoogleId == googleId && !u.IsDeleted, cancellationToken);

    // Carga el usuario con sus perfiles y tokens para operaciones de auth post-login
    public Task<User?> GetByIdWithProfilesAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Users
            .Include(u => u.StudentProfile)
            .Include(u => u.VendorProfile)
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        => await _context.Users.AddAsync(user, cancellationToken);

    public void Update(User user)
        => _context.Users.Update(user);

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
        => _context.Users.AnyAsync(u => u.Email.ToLower() == email.ToLower() && !u.IsDeleted, cancellationToken);
}
