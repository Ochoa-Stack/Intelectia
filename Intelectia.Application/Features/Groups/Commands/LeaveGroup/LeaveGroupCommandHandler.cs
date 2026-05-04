using MediatR;
using Intelectia.Application.Common.Exceptions;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces;
using Intelectia.Domain.Interfaces.Repositories;

namespace Intelectia.Application.Features.Groups.Commands.LeaveGroup;

public class LeaveGroupCommandHandler : IRequestHandler<LeaveGroupCommand>
{
    private readonly IGroupRepository _groupRepository;
    private readonly IUnitOfWork      _unitOfWork;

    public LeaveGroupCommandHandler(IGroupRepository groupRepository, IUnitOfWork unitOfWork)
    {
        _groupRepository = groupRepository;
        _unitOfWork      = unitOfWork;
    }

    public async Task Handle(LeaveGroupCommand request, CancellationToken cancellationToken)
    {
        var group = await _groupRepository.GetByIdWithMembersAsync(
            request.GroupId, cancellationToken);

        if (group is null)
            throw new NotFoundException(nameof(StudyGroup), request.GroupId);

        var member = group.Members.FirstOrDefault(
            m => m.UserId == request.UserId && !m.IsDeleted);

        if (member is null)
            throw new NotFoundException(nameof(GroupMember), request.UserId);

        // El creador del grupo no puede abandonarlo
        if (group.CreatedByUserId == request.UserId)
            throw new ConflictException("El creador del grupo no puede abandonarlo. Elimina el grupo si ya no lo necesitas.");

        // Soft delete de la membresía
        member.IsDeleted = true;
        member.DeletedAt = DateTime.UtcNow;

        _groupRepository.Update(group);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
