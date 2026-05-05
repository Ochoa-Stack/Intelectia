namespace Intelectia.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // Persiste todos los cambios pendientes en la unidad de trabajo actual
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
