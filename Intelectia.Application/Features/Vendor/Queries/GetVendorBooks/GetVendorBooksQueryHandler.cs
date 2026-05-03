using MediatR;
using Microsoft.EntityFrameworkCore;
using Intelectia.Application.Common.Interfaces;
using Intelectia.Shared.DTOs.Vendor;

namespace Intelectia.Application.Features.Vendor.Queries.GetVendorBooks;

public class GetVendorBooksQueryHandler
    : IRequestHandler<GetVendorBooksQuery, IReadOnlyList<VendorBookDto>>
{
    private readonly IApplicationDbContext _context;

    public GetVendorBooksQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<VendorBookDto>> Handle(
        GetVendorBooksQuery request, CancellationToken cancellationToken)
    {
        // Traemos todos los libros del vendedor con su categoría
        return await _context.Books
            .Include(b => b.Category)
            .Where(b => b.VendorProfileId == request.VendorProfileId && !b.IsDeleted)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => new VendorBookDto
            {
                Id           = b.Id,
                Title        = b.Title,
                Author       = b.Author,
                CategoryName = b.Category.Name,
                Price        = b.Price,
                Format       = b.Format.ToString(),
                Status       = b.Status.ToString(),
                AverageRating = b.AverageRating,
                ReviewCount  = b.ReviewCount,
                CreatedAt    = b.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }
}
