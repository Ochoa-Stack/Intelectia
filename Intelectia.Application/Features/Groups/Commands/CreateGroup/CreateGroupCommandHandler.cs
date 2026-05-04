using MediatR;
using Intelectia.Application.Common.Exceptions;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Enums;
using Intelectia.Domain.Interfaces;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Shared.DTOs.Groups;

namespace Intelectia.Application.Features.Groups.Commands.CreateGroup;

public class CreateGroupCommandHandler : IRequestHandler<CreateGroupCommand, GroupDto>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IUserRepository  _userRepository;
    private readonly IUnitOfWork      _unitOfWork;

    public CreateGroupCommandHandler(
        IGroupRepository groupRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _groupRepository = groupRepository;
        _userRepository  = userRepository;
        _unitOfWork      = unitOfWork;
    }

    public async Task<GroupDto> Handle(
        CreateGroupCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdWithProfilesAsync(request.UserId, cancellationToken);
        if (user is null)
            throw new NotFoundException(nameof(User), request.UserId);

        // Creamos el grupo y agregamos al creador como Admin automáticamente
        var group = new StudyGroup
        {
            Name              = request.Name,
            Description       = request.Description,
            IsPublic          = request.IsPublic,
            CreatedByUserId   = request.UserId
        };

        var adminMember = new GroupMember
        {
            UserId   = request.UserId,
            Role     = GroupMemberRole.Admin,
            JoinedAt = DateTime.UtcNow
        };

        group.Members.Add(adminMember);

        await _groupRepository.AddAsync(group, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new GroupDto
        {
            Id            = group.Id,
            Name          = group.Name,
            Description   = group.Description,
            IsPublic      = group.IsPublic,
            MemberCount   = 1,
            CreatedByName = $"{user.FirstName} {user.LastName}",
            CreatedAt     = group.CreatedAt,
            UserRole      = GroupMemberRole.Admin.ToString()
        };
    }
}
