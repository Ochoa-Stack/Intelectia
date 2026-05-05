using MediatR;
using Intelectia.Domain.Enums;
using Intelectia.Shared.DTOs.Marketplace;

namespace Intelectia.Application.Features.Marketplace.Queries.GetBooks;

public record GetBooksQuery(
    int         Page       = 1,
    int         PageSize   = 12,
    string?     Search     = null,
    Guid?       CategoryId = null,
    BookFormat? Format     = null,
    decimal?    MinPrice   = null,
    decimal?    MaxPrice   = null,
    string?     SortBy     = null
) : IRequest<PagedResult<BookSummaryDto>>;
