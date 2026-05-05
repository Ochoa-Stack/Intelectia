using FluentValidation;

namespace Intelectia.Application.Features.Marketplace.Commands.AddReview;

public class AddReviewCommandValidator : AbstractValidator<AddReviewCommand>
{
    public AddReviewCommandValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("La calificación debe ser entre 1 y 5.");

        RuleFor(x => x.Comment)
            .MaximumLength(2000)
            .WithMessage("El comentario no puede superar 2000 caracteres.")
            .When(x => x.Comment is not null);
    }
}
