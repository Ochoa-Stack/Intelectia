using AutoMapper;
using MediatR;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Shared.DTOs.Library;

namespace Intelectia.Application.Features.Library.Commands.CreateNote;

public class CreateNoteCommandHandler : IRequestHandler<CreateNoteCommand, NoteDto>
{
    private readonly INoteRepository _noteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateNoteCommandHandler(
        INoteRepository noteRepository, IUnitOfWork unitOfWork, IMapper mapper)
    {
        _noteRepository = noteRepository;
        _unitOfWork     = unitOfWork;
        _mapper         = mapper;
    }

    public async Task<NoteDto> Handle(CreateNoteCommand request, CancellationToken cancellationToken)
    {
        var note = new Note
        {
            UserId          = request.UserId,
            BookId          = request.BookId,
            Title           = request.Title,
            Content         = request.Content,
            PageNumber      = request.PageNumber,
            HighlightedText = request.HighlightedText,
            HighlightColor  = request.HighlightColor
        };

        await _noteRepository.AddAsync(note, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<NoteDto>(note);
    }
}
