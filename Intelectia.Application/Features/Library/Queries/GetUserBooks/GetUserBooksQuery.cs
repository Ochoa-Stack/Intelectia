using MediatR;
using Intelectia.Shared.DTOs.Library;

namespace Intelectia.Application.Features.Library.Queries.GetUserBooks;

public record GetUserBooksQuery(Guid UserId) : IRequest<IReadOnlyList<UserBookDto>>;
