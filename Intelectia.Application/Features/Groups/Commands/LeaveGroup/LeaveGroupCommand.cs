using MediatR;

namespace Intelectia.Application.Features.Groups.Commands.LeaveGroup;

public record LeaveGroupCommand(Guid GroupId, Guid UserId) : IRequest;
