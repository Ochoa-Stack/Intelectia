using MediatR;
using Intelectia.Shared.DTOs.Groups;

namespace Intelectia.Application.Features.Groups.Commands.CreateGroup;

public record CreateGroupCommand(
    Guid UserId,
    string Name,
    string? Description,
    bool IsPublic
) : IRequest<GroupDto>;
