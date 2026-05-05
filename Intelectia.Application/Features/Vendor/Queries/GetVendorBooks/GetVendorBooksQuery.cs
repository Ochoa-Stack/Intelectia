using MediatR;
using Intelectia.Shared.DTOs.Vendor;

namespace Intelectia.Application.Features.Vendor.Queries.GetVendorBooks;

public record GetVendorBooksQuery(Guid VendorProfileId) : IRequest<IReadOnlyList<VendorBookDto>>;
