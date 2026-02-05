using FluentValidation;

namespace MicroCommerce.ApiService.Features.Cart.Application.Commands.AddToCart;

public sealed class AddToCartValidator
    : AbstractValidator<AddToCartCommand>
{
    public AddToCartValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required.");

        RuleFor(x => x.ProductName)
            .NotEmpty().WithMessage("Product name is required.");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Unit price must be greater than zero.");

        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, 99).WithMessage("Quantity must be between 1 and 99.");
    }
}
