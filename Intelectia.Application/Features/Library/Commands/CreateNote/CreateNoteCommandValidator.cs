using FluentValidation;

namespace Intelectia.Application.Features.Library.Commands.CreateNote;

public class CreateNoteCommandValidator : AbstractValidator<CreateNoteCommand>
{
    public CreateNoteCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es obligatorio.")
            .MaximumLength(200).WithMessage("El título no puede superar 200 caracteres.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("El contenido es obligatorio.")
            .MaximumLength(10000).WithMessage("El contenido no puede superar 10,000 caracteres.");
    }
}
