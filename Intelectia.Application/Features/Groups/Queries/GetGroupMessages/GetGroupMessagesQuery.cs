using MediatR;
using Intelectia.Shared.DTOs.Groups;

namespace Intelectia.Application.Features.Groups.Queries.GetGroupMessages;

public record GetGroupMessagesQuery(
    Guid GroupId,
    Guid UserId,
    int Page = 1,
    int PageSize = 30
) : IRequest<PagedMessagesDto>;
