using Microsoft.EntityFrameworkCore;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Enums;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Infrastructure.Persistence;

namespace Intelectia.Infrastructure.Repositories;

public class BookRepository : IBookRepository
{
    private readonly AppDbContext _context;

    public BookRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(IReadOnlyList<Book> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        string? search,
        Guid? categoryId,
        BookFormat? format,
        decimal? minPrice,
        decimal? maxPrice,
        string? sortBy,
        CancellationToken cancellationToken = default)
    {
        // Construimos la query base con los datos necesarios para las tarjetas del catálogo
        var query = _context.Books
            .Include(b => b.Category)
            .Include(b => b.VendorProfile)
            .Where(b => b.Status == BookStatus.Active)
            .AsQueryable();

        // Aplicamos el filtro de búsqueda por título o autor
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(b =>
                b.Title.ToLower().Contains(search.ToLower()) ||
                b.Author.ToLower().Contains(search.ToLower()));

        // Filtramos por categoría si se especificó
        if (categoryId.HasValue)
            query = query.Where(b => b.CategoryId == categoryId.Value);

        // Filtramos por formato si se especificó
        if (format.HasValue)
            query = query.Where(b => b.Format == format.Value);

        // Aplicamos rango de precios si se especificaron
        if (minPrice.HasValue)
            query = query.Where(b => b.Price >= minPrice.Value);
        if (maxPrice.HasValue)
            query = query.Where(b => b.Price <= maxPrice.Value);

        // Contamos el total antes de paginar para devolver la paginación correcta
        var totalCount = await query.CountAsync(cancellationToken);

        // Ordenamos según el criterio solicitado
        query = sortBy?.ToLower() switch
        {
            "price_asc"  => query.OrderBy(b => b.Price),
            "price_desc" => query.OrderByDescending(b => b.Price),
            "rating"     => query.OrderByDescending(b => b.AverageRating),
            "newest"     => query.OrderByDescending(b => b.CreatedAt),
            _            => query.OrderByDescending(b => b.CreatedAt)
        };

        // Aplicamos la paginación
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    // Carga el libro completo con reseñas y datos del vendedor para la vista de detalle
    public Task<Book?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Books
            .Include(b => b.Category)
            .Include(b => b.VendorProfile)
            .Include(b => b.Reviews)
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public async Task AddAsync(Book book, CancellationToken cancellationToken = default)
        => await _context.Books.AddAsync(book, cancellationToken);

    public void Update(Book book)
        => _context.Books.Update(book);
}
