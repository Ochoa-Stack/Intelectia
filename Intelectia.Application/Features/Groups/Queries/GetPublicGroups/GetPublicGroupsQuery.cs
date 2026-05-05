using MediatR;
using Intelectia.Shared.DTOs.Groups;

namespace Intelectia.Application.Features.Groups.Queries.GetPublicGroups;

public record GetPublicGroupsQuery(Guid UserId, string? Search = null)
    : IRequest<IReadOnlyList<GroupDto>>;
