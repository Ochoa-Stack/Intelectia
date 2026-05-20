using MediatR;
using Intelectia.Application.Common.Interfaces;
using Intelectia.Domain.Entities;
using Intelectia.Domain.Interfaces;
using Intelectia.Shared.DTOs.Library;

namespace Intelectia.Application.Features.Library.Commands.TranslateText;

public class TranslateTextCommandHandler : IRequestHandler<TranslateTextCommand, TranslationDto>
{
    private readonly ITranslationService   _translationService;
    private readonly IRepository<TranslationHistory> _translationHistoryRepository;
    private readonly IUnitOfWork           _unitOfWork;

    public TranslateTextCommandHandler(
        ITranslationService translationService,
        IRepository<TranslationHistory> translationHistoryRepository,
        IUnitOfWork unitOfWork)
    {
        _translationService           = translationService;
        _translationHistoryRepository = translationHistoryRepository;
        _unitOfWork                   = unitOfWork;
    }

    public async Task<TranslationDto> Handle(
        TranslateTextCommand request, CancellationToken cancellationToken)
    {
        // Enviamos el texto a DeepL
        var translatedText = await _translationService.TranslateAsync(
            request.Text,
            request.TargetLanguage,
            request.SourceLanguage,
            cancellationToken);

        // Guardamos la traducción en el historial del usuario
        var history = new TranslationHistory
        {
            UserId         = request.UserId,
            SourceText     = request.Text,
            TranslatedText = translatedText,
            SourceLanguage = request.SourceLanguage ?? "auto",
            TargetLanguage = request.TargetLanguage,
            BookId         = request.BookId
        };

        await _translationHistoryRepository.AddAsync(history, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TranslationDto
        {
            SourceText     = request.Text,
            TranslatedText = translatedText,
            SourceLanguage = request.SourceLanguage ?? "auto",
            TargetLanguage = request.TargetLanguage
        };
    }
}
