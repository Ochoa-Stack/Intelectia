using Intelectia.Domain.Entities;
using Intelectia.Domain.Enums;

namespace Intelectia.Domain.Interfaces.Repositories;

public interface IBookRepository
{
    // Trae una página de libros con filtros opcionales
    Task<(IReadOnlyList<Book> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        Guid? categoryId,
        BookFormat? format,
        decimal? minPrice,
        decimal? maxPrice,
        string? sortBy,
        CancellationToken cancellationToken = default);

    // Trae un libro por ID con sus reseñas y categoría
    Task<Book?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    // Trae todos los libros de un vendedor con su categoría
    Task<IReadOnlyList<Book>> GetVendorBooksAsync(Guid vendorProfileId, CancellationToken cancellationToken = default);

    // Agrega un libro nuevo
    Task AddAsync(Book book, CancellationToken cancellationToken = default);

    // Marca el libro como modificado
    void Update(Book book);
}
