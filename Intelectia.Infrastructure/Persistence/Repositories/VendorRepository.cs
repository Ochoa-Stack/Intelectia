using Intelectia.Domain.Entities;
using Intelectia.Domain.Enums;
using Intelectia.Domain.Interfaces;
using Intelectia.Shared.DTOs.Vendor;
using Microsoft.EntityFrameworkCore;

namespace Intelectia.Infrastructure.Persistence.Repositories;

// Encapsulamos la consulta compleja con includes y agrupaciones
public class VendorRepository : Repository<VendorProfile>, IVendorRepository
{
    public VendorRepository(AppDbContext context) : base(context) { }

    public async Task<VendorStatsDto> GetVendorStatsAsync(Guid vendorProfileId, CancellationToken cancellationToken = default)
    {
        // Buscamos los libros asociados al vendedor
        var books = await _context.Books
            .Where(b => b.VendorProfileId == vendorProfileId && !b.IsDeleted)
            .ToListAsync(cancellationToken);

        var bookIds = books.Select(b => b.Id).ToList();

        // Cruzamos los elementos del pedido con el estado del pago
        var salesData = await _context.OrderItems
            .Include(oi => oi.Order)
            .Where(oi => bookIds.Contains(oi.BookId) && oi.Order.Status == OrderStatus.Paid && !oi.IsDeleted)
            .GroupBy(oi => oi.BookId)
            .Select(g => new { BookId = g.Key, Count = g.Count(), Revenue = g.Sum(oi => oi.PriceSnapshot) })
            .ToListAsync(cancellationToken);

        var topBookId = salesData.OrderByDescending(s => s.Count).FirstOrDefault()?.BookId;
        var topBookTitle = topBookId.HasValue ? books.FirstOrDefault(b => b.Id == topBookId.Value)?.Title : null;

        return new VendorStatsDto
        {
            TotalBooks = books.Count,
            TotalSales = salesData.Sum(s => s.Count),
            TotalRevenue = salesData.Sum(s => s.Revenue),
            AverageRating = books.Count > 0 ? Math.Round(books.Average(b => b.AverageRating), 1) : 0,
            TopBookTitle = topBookTitle
        };
    }
}
