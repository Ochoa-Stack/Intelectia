using MediatR;
using Microsoft.EntityFrameworkCore;
using Intelectia.Application.Common.Interfaces;
using Intelectia.Domain.Enums;
using Intelectia.Shared.DTOs.Vendor;

namespace Intelectia.Application.Features.Vendor.Queries.GetVendorStats;

public class GetVendorStatsQueryHandler : IRequestHandler<GetVendorStatsQuery, VendorStatsDto>
{
    private readonly IApplicationDbContext _context;

    public GetVendorStatsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<VendorStatsDto> Handle(
        GetVendorStatsQuery request, CancellationToken cancellationToken)
    {
        // Traemos los libros del vendedor para calcular estadísticas
        var books = await _context.Books
            .Where(b => b.VendorProfileId == request.VendorProfileId && !b.IsDeleted)
            .ToListAsync(cancellationToken);

        var bookIds = books.Select(b => b.Id).ToList();

        // Contamos las ventas confirmadas cruzando OrderItems con pedidos pagados
        var salesData = await _context.OrderItems
            .Include(oi => oi.Order)
            .Where(oi =>
                bookIds.Contains(oi.BookId) &&
                oi.Order.Status == OrderStatus.Paid &&
                !oi.IsDeleted)
            .GroupBy(oi => oi.BookId)
            .Select(g => new
            {
                BookId   = g.Key,
                Count    = g.Count(),
                Revenue  = g.Sum(oi => oi.PriceSnapshot)
            })
            .ToListAsync(cancellationToken);

        // Buscamos el libro más vendido
        var topBookId = salesData
            .OrderByDescending(s => s.Count)
            .FirstOrDefault()?.BookId;

        var topBookTitle = topBookId.HasValue
            ? books.FirstOrDefault(b => b.Id == topBookId.Value)?.Title
            : null;

        return new VendorStatsDto
        {
            TotalBooks    = books.Count,
            TotalSales    = salesData.Sum(s => s.Count),
            TotalRevenue  = salesData.Sum(s => s.Revenue),
            AverageRating = books.Count > 0
                ? Math.Round(books.Average(b => b.AverageRating), 1)
                : 0,
            TopBookTitle  = topBookTitle
        };
    }
}
