using MediatR;
using Intelectia.Shared.DTOs.Library;

namespace Intelectia.Application.Features.Library.Commands.TranslateText;

public record TranslateTextCommand(
    Guid UserId,
    string Text,
    string TargetLanguage,
    string? SourceLanguage,
    Guid? BookId
) : IRequest<TranslationDto>;
