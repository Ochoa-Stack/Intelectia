using MediatR;
using Intelectia.Application.Common.Exceptions;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces;
using Intelectia.Domain.Interfaces.Repositories;

namespace Intelectia.Application.Features.Library.Commands.DeleteNote;

public class DeleteNoteCommandHandler : IRequestHandler<DeleteNoteCommand>
{
    private readonly INoteRepository _noteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteNoteCommandHandler(INoteRepository noteRepository, IUnitOfWork unitOfWork)
    {
        _noteRepository = noteRepository;
        _unitOfWork     = unitOfWork;
    }

    public async Task Handle(DeleteNoteCommand request, CancellationToken cancellationToken)
    {
        var note = await _noteRepository.GetByIdAsync(request.NoteId, cancellationToken);

        // Verificamos que la nota exista y pertenezca al usuario; IDOR guard
        if (note is null || note.UserId != request.UserId)
            throw new NotFoundException(nameof(Note), request.NoteId);

        _noteRepository.Delete(note);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
