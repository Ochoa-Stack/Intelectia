using MediatR;

namespace Intelectia.Application.Features.Library.Commands.DeleteNote;

public record DeleteNoteCommand(Guid NoteId, Guid UserId) : IRequest;
