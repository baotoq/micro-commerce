using FluentValidation;

namespace MicroCommerce.ApiService.Features.Catalog.Application.Commands.ChangeProductStatus;

public sealed class ChangeProductStatusCommandValidator
    : AbstractValidator<ChangeProductStatusCommand>
{
    public ChangeProductStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(BeAValidStatus).WithMessage("Status must be 'Draft' or 'Published'.");
    }

    private static bool BeAValidStatus(string status)
    {
        return status.Equals("Draft", StringComparison.OrdinalIgnoreCase) ||
               status.Equals("Published", StringComparison.OrdinalIgnoreCase);
    }
}

