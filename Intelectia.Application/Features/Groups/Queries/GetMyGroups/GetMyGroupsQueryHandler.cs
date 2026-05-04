using MediatR;
using Intelectia.Domain.Enums;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Shared.DTOs.Groups;

namespace Intelectia.Application.Features.Groups.Queries.GetMyGroups;

public class GetMyGroupsQueryHandler : IRequestHandler<GetMyGroupsQuery, IReadOnlyList<GroupDto>>
{
    private readonly IGroupRepository _groupRepository;

    public GetMyGroupsQueryHandler(IGroupRepository groupRepository)
    {
        _groupRepository = groupRepository;
    }

    public async Task<IReadOnlyList<GroupDto>> Handle(
        GetMyGroupsQuery request, CancellationToken cancellationToken)
    {
        var groups = await _groupRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        return groups.Select(g => new GroupDto
        {
            Id            = g.Id,
            Name          = g.Name,
            Description   = g.Description,
            IsPublic      = g.IsPublic,
            MemberCount   = g.Members.Count(m => !m.IsDeleted),
            CreatedByName = $"{g.CreatedByUser.FirstName} {g.CreatedByUser.LastName}",
            CreatedAt     = g.CreatedAt,
            UserRole      = g.Members
                .FirstOrDefault(m => m.UserId == request.UserId && !m.IsDeleted)
                ?.Role.ToString()
        }).ToList();
    }
}
