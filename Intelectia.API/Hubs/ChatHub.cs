using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Intelectia.Application.Common.Interfaces;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces;
using Intelectia.Shared.DTOs.Groups;

namespace Intelectia.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IApplicationDbContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public ChatHub(IApplicationDbContext context, IUnitOfWork unitOfWork)
    {
        _context    = context;
        _unitOfWork = unitOfWork;
    }

    // El cliente se une al canal SignalR del grupo
    public async Task JoinGroup(string groupId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
    }

    // El cliente abandona el canal del grupo
    public async Task LeaveGroup(string groupId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
    }

    // El cliente envía un mensaje al grupo
    public async Task SendMessage(string groupId, string content)
    {
        var userId = GetUserId();

        // Verificamos que el usuario sea miembro del grupo
        var groupGuid = Guid.Parse(groupId);
        var isMember = await _context.GroupMembers
            .AnyAsync(m => m.GroupId == groupGuid &&
                           m.UserId == userId &&
                           !m.IsDeleted);

        if (!isMember)
        {
            await Clients.Caller.SendAsync("Error", "No eres miembro de este grupo.");
            return;
        }

        // Obtenemos los datos del usuario para construir el DTO
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null) return;

        // Persistimos el mensaje en la base de datos
        var message = new GroupMessage
        {
            GroupId = groupGuid,
            UserId  = userId,
            Content = content.Trim()
        };

        await _context.GroupMessages.AddAsync(message);
        await _unitOfWork.SaveChangesAsync(default);

        // Transmitimos el mensaje a todos los miembros conectados al grupo
        var dto = new GroupMessageDto
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
