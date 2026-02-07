using FluentValidation;

namespace MicroCommerce.ApiService.Features.Inventory.Application.Commands.AdjustStock;

public sealed class AdjustStockCommandValidator
    : AbstractValidator<AdjustStockCommand>
{
    public AdjustStockCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.Adjustment)
            .NotEqual(0).WithMessage("Adjustment must not be zero.");
    }
}
