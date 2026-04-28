using Microsoft.EntityFrameworkCore;
using Intelectia.Domain.Entities;

namespace Intelectia.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // Acceso a los refresh tokens para validación y rotación
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
