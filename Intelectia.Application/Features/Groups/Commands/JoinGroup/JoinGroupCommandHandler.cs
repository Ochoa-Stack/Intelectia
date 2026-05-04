using MediatR;
using Intelectia.Application.Common.Exceptions;
using Intelectia.Application.Common.Interfaces;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Enums;
using Intelectia.Domain.Interfaces;
using Intelectia.Domain.Interfaces.Repositories;

namespace Intelectia.Application.Features.Groups.Commands.JoinGroup;

public class JoinGroupCommandHandler : IRequestHandler<JoinGroupCommand>
{
    private readonly IGroupRepository      _groupRepository;
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork           _unitOfWork;

    public JoinGroupCommandHandler(
        IGroupRepository groupRepository,
        IApplicationDbContext context,
        IUnitOfWork unitOfWork)
    {
        _groupRepository = groupRepository;
        _context         = context;
        _unitOfWork      = unitOfWork;
    }

    public async Task Handle(JoinGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByIdWithMembersAsync(
            request.GroupId, cancellationToken);

        if (group is null)
            throw new NotFoundException(nameof(StudyGroup), request.GroupId);

        if (!group.IsPublic)
            throw new ConflictException("Este grupo es privado.");

        // Verificamos que no sea miembro ya
        var alreadyMember = group.Members.Any(
            m => m.UserId == request.UserId && !m.IsDeleted);

        if (alreadyMember)
            throw new ConflictException("Ya eres miembro de este grupo.");

        // Agregamos al usuario como miembro regular
        await _context.GroupMembers.AddAsync(new GroupMember
        {
            GroupId  = request.GroupId,
            UserId   = request.UserId,
            Role     = GroupMemberRole.Member,
            JoinedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
