using MediatR;
using Intelectia.Shared.DTOs.Marketplace;

namespace Intelectia.Application.Features.Marketplace.Queries.GetBookById;

public record GetBookByIdQuery(Guid Id) : IRequest<BookDetailDto>;
