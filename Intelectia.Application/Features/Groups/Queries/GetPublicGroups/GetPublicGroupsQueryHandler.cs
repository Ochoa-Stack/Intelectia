using MediatR;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Shared.DTOs.Groups;

namespace Intelectia.Application.Features.Groups.Queries.GetPublicGroups;

public class GetPublicGroupsQueryHandler
    : IRequestHandler<GetPublicGroupsQuery, IReadOnlyList<GroupDto>>
{
    private readonly IGroupRepository _groupRepository;

    public GetPublicGroupsQueryHandler(IGroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
    }

    public async Task<IReadOnlyList<GroupDto>> Handle(
        GetPublicGroupsQuery request, CancellationToken cancellationToken)
    {
        var groups = await _groupRepository.GetPublicGroupsAsync(
            request.UserId, request.Search, cancellationToken);

        return groups.Select(g => new GroupDto
        {
            Id            = g.Id,
            Name          = g.Name,
            Description   = g.Description,
            IsPublic      = g.IsPublic,
            MemberCount   = g.Members.Count(m => !m.IsDeleted),
            CreatedByName = $"{g.CreatedByUser.FirstName} {g.CreatedByUser.LastName}",
            CreatedAt     = g.CreatedAt
        }).ToList();
    }
}
