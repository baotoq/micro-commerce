using FluentValidation;

namespace MicroCommerce.ApiService.Features.Inventory.Application.Commands.ReserveStock;

public sealed class ReserveStockCommandValidator
    : AbstractValidator<ReserveStockCommand>
{
    public ReserveStockCommandValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
    }
}
