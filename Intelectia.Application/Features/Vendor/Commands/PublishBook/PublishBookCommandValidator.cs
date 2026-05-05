using FluentValidation;

namespace Intelectia.Application.Features.Vendor.Commands.PublishBook;

public class PublishBookCommandValidator : AbstractValidator<PublishBookCommand>
{
    public PublishBookCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("El título es obligatorio.")
            .MaximumLength(300).WithMessage("El título no puede superar 300 caracteres.");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("El autor es obligatorio.")
            .MaximumLength(200).WithMessage("El autor no puede superar 200 caracteres.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("La descripción es obligatoria.")
            .MaximumLength(3000).WithMessage("La descripción no puede superar 3000 caracteres.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("El precio debe ser mayor a cero.");

        RuleFor(x => x.PublishedYear)
            .InclusiveBetween(1800, DateTime.UtcNow.Year)
            .WithMessage("El año de publicación no es válido.");

        RuleFor(x => x.PageCount)
            .GreaterThan(0).WithMessage("El número de páginas debe ser mayor a cero.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("La categoría es obligatoria.");
    }
}
