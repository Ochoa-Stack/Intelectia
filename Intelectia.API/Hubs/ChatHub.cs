using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces;

namespace Intelectia.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IRepository<GroupMember> _groupMemberRepository;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<GroupMessage> _groupMessageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChatHub(
        IRepository<GroupMember> groupMemberRepository,
        IRepository<User> userRepository,
        IRepository<GroupMessage> groupMessageRepository,
        IUnitOfWork unitOfWork)
    {
        _groupMemberRepository  = groupMemberRepository;
        _userRepository         = userRepository;
        _groupMessageRepository = groupMessageRepository;
        _unitOfWork             = unitOfWork;
    }

    // El cliente se une al canal SignalR del grupo
    public async Task JoinGroup(string groupId)
    {
        var userId = GetUserId();
        var groupGuid = Guid.Parse(groupId);
        var isMember = await _groupMemberRepository
            .AnyAsync(m => m.GroupId == groupGuid && m.UserId == userId);

        if (!isMember)
        {
            await Clients.Caller.SendAsync("Error", "No eres miembro de este grupo.");
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
    }

    // El cliente abandona el canal del grupo
    public async Task LeaveGroup(string groupId)
    {
        var userId = GetUserId();
        var groupGuid = Guid.Parse(groupId);
        var isMember = await _groupMemberRepository
            .AnyAsync(m => m.GroupId == groupGuid && m.UserId == userId);

        if (isMember)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
        }
    }

    // El cliente envía un mensaje al grupo
    public async Task SendMessage(string groupId, string content)
    {
        var userId = GetUserId();

        // Verificamos que el usuario sea miembro del grupo
        var groupGuid = Guid.Parse(groupId);
        var isMember = await _groupMemberRepository
            .AnyAsync(m => m.GroupId == groupGuid &&
                           m.UserId == userId &&
                           !m.IsDeleted);

        if (!isMember)
        {
            await Clients.Caller.SendAsync("Error", "No eres miembro de este grupo.");
            return;
        }

        // Obtenemos los datos del usuario para construir el DTO
        var user = await _userRepository
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null) return;

        // Persistimos el mensaje en la base de datos
        var message = new GroupMessage
        {
            GroupId = groupGuid,
            UserId  = userId,
            Content = content.Trim()
        };

        await _groupMessageRepository.AddAsync(message);
        await _unitOfWork.SaveChangesAsync(default);

        // Transmitimos el mensaje a todos los miembros conectados al grupo
        var dto = new Intelectia.Shared.DTOs.Groups.GroupMessageDto
        {
            Id           = message.Id,
            GroupId      = message.GroupId,
            UserId       = message.UserId,
            UserFullName = $"{user.FirstName} {user.LastName}",
            Content      = message.Content,
            IsEdited     = false,
            CreatedAt    = message.CreatedAt
        };

        await Clients.Group(groupId).SendAsync("ReceiveMessage", dto);
    }

    // Notifica a los miembros del grupo que el usuario está escribiendo
    public async Task SendTyping(string groupId)
    {
        var userId = GetUserId();
        await Clients.OthersInGroup(groupId)
            .SendAsync("UserTyping", userId.ToString());
    }

    private Guid GetUserId()
    {
        var claim = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new HubException("Token inválido.");
        return Guid.Parse(claim);
    }
}
