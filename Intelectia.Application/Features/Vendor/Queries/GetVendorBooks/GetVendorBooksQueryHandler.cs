using MediatR;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Shared.DTOs.Vendor;

namespace Intelectia.Application.Features.Vendor.Queries.GetVendorBooks;

public class GetVendorBooksQueryHandler
    : IRequestHandler<GetVendorBooksQuery, IReadOnlyList<VendorBookDto>>
{
    private readonly IBookRepository _bookRepository;

    public GetVendorBooksQueryHandler(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<IReadOnlyList<VendorBookDto>> Handle(
        GetVendorBooksQuery request, CancellationToken cancellationToken)
    {
        // Traemos todos los libros del vendedor con su categoría; abstraído en el repositorio
        var books = await _bookRepository.GetVendorBooksAsync(request.VendorProfileId, cancellationToken);

        return books.Select(b => new VendorBookDto
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
        }).ToList();
    }
}
