using MediatR;

namespace Intelectia.Application.Features.Groups.Commands.JoinGroup;

public record JoinGroupCommand(Guid GroupId, Guid UserId) : IRequest;
