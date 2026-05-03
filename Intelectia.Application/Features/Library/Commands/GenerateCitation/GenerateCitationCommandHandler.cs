using AutoMapper;
using MediatR;
using Intelectia.Application.Common.Exceptions;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Enums;
using Intelectia.Domain.Interfaces;
using Intelectia.Domain.Interfaces.Repositories;
using Intelectia.Shared.DTOs.Library;

namespace Intelectia.Application.Features.Library.Commands.GenerateCitation;

public class GenerateCitationCommandHandler : IRequestHandler<GenerateCitationCommand, CitationDto>
{
    private readonly IBookRepository     _bookRepository;
    private readonly ICitationRepository _citationRepository;
    private readonly IUnitOfWork         _unitOfWork;
    private readonly IMapper             _mapper;

    public GenerateCitationCommandHandler(
        IBookRepository bookRepository,
        ICitationRepository citationRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _bookRepository     = bookRepository;
        _citationRepository = citationRepository;
        _unitOfWork         = unitOfWork;
        _mapper             = mapper;
    }

    public async Task<CitationDto> Handle(
        GenerateCitationCommand request, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdWithDetailsAsync(request.BookId, cancellationToken);

        if (book is null)
            throw new NotFoundException(nameof(Book), request.BookId);

        // Generamos el texto de la cita según el formato académico solicitado
        var citationText = request.Format switch
        {
            CitationFormat.APA     => GenerateApa(book, request.PageNumber),
            CitationFormat.MLA     => GenerateMla(book, request.PageNumber),
            CitationFormat.Chicago => GenerateChicago(book, request.PageNumber),
            CitationFormat.IEEE    => GenerateIeee(book, request.PageNumber),
            _                      => GenerateApa(book, request.PageNumber)
        };

        var citation = new Citation
        {
            UserId        = request.UserId,
            BookId        = request.BookId,
            Format        = request.Format,
            GeneratedText = citationText,
            PageNumber    = request.PageNumber
        };

        await _citationRepository.AddAsync(citation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Recargamos con Book incluido para que el mapper tenga BookTitle
        var saved = await _citationRepository.GetByIdAsync(citation.Id, cancellationToken)
            ?? citation;

        return _mapper.Map<CitationDto>(saved);
    }

    // Autor (Año). Título. Editorial. (APA)
    private static string GenerateApa(Book book, int? page)
    {
        var text = $"{book.Author} ({book.PublishedYear}). {book.Title}. Intelectia Editorial.";
        return page.HasValue ? $"{text} p. {page}." : text;
    }

    // Autor. "Título". Editorial, Año. (MLA)
    private static string GenerateMla(Book book, int? page)
    {
        var text = $"{book.Author}. \"{book.Title}\". Intelectia Editorial, {book.PublishedYear}.";
        return page.HasValue ? $"{text} p. {page}." : text;
    }

    // Autor. Título. Editorial, Año. (Chicago)
    private static string GenerateChicago(Book book, int? page)
    {
        var text = $"{book.Author}. {book.Title}. Intelectia Editorial, {book.PublishedYear}.";
        return page.HasValue ? $"{text} {page}." : text;
    }

    // Autor, "Título," Editorial, Año. (IEEE)
    private static string GenerateIeee(Book book, int? page)
    {
        var text = $"{book.Author}, \"{book.Title},\" Intelectia Editorial, {book.PublishedYear}.";
        return page.HasValue ? $"{text} p. {page}." : text;
    }
}
