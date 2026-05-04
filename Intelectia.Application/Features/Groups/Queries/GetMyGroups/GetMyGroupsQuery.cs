using MediatR;
using Intelectia.Shared.DTOs.Groups;

namespace Intelectia.Application.Features.Groups.Queries.GetMyGroups;

public record GetMyGroupsQuery(Guid UserId) : IRequest<IReadOnlyList<GroupDto>>;
