using MediatR;
using Intelectia.Application.Common.Exceptions;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Shared.DTOs.Groups;

namespace Intelectia.Application.Features.Groups.Queries.GetGroupMessages;

public class GetGroupMessagesQueryHandler
    : IRequestHandler<GetGroupMessagesQuery, PagedMessagesDto>
{
    private readonly IGroupRepository        _groupRepository;
    private readonly IGroupMessageRepository _messageRepository;

    public GetGroupMessagesQueryHandler(
        IGroupRepository groupRepository,
        IGroupMessageRepository messageRepository)
    {
        _groupRepository   = groupRepository;
        _messageRepository = messageRepository;
    }

    public async Task<PagedMessagesDto> Handle(
        GetGroupMessagesQuery request, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByIdWithMembersAsync(
            request.GroupId, cancellationToken);

        if (group is null)
            throw new NotFoundException(nameof(StudyGroup), request.GroupId);

        // Solo miembros pueden leer el historial
        var isMember = group.Members.Any(
            m => m.UserId == request.UserId && !m.IsDeleted);

        if (!isMember)
            throw new UnauthorizedException("No eres miembro de este grupo.");

        var (items, totalCount) = await _messageRepository.GetPagedByGroupIdAsync(
            request.GroupId, request.Page, request.PageSize, cancellationToken);

        return new PagedMessagesDto
        {
            Items = items.Select(m => new GroupMessageDto
            {
                Id           = m.Id,
                GroupId      = m.GroupId,
                UserId       = m.UserId,
                UserFullName = $"{m.User.FirstName} {m.User.LastName}",
                Content      = m.Content,
                IsEdited     = m.IsEdited,
                CreatedAt    = m.CreatedAt
            }).ToList(),
            Page       = request.Page,
            PageSize   = request.PageSize,
            TotalCount = totalCount
        };
    }
}
