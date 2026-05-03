using FluentValidation;

namespace Intelectia.Application.Features.Vendor.Commands.BecomeVendor;

public class BecomeVendorCommandValidator : AbstractValidator<BecomeVendorCommand>
{
    public BecomeVendorCommandValidator()
    {
        RuleFor(x => x.BusinessName)
            .NotEmpty().WithMessage("El nombre comercial es obligatorio.")
            .MaximumLength(200).WithMessage("El nombre comercial no puede superar 200 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("La descripción no puede superar 1000 caracteres.")
            .When(x => x.Description is not null);
    }
}
