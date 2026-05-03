using MediatR;
using Intelectia.Domain.Enums;
using Intelectia.Shared.DTOs.Vendor;

namespace Intelectia.Application.Features.Vendor.Commands.PublishBook;

public record PublishBookCommand(
    Guid VendorProfileId,
    string Title,
    string Author,
    string Description,
    string ISBN,
    int PublishedYear,
    int PageCount,
    string Language,
    decimal Price,
    BookFormat Format,
    Guid CategoryId
) : IRequest<VendorBookDto>;
