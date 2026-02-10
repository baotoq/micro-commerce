using FluentValidation;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Commands.SubmitOrder;

public sealed class SubmitOrderCommandValidator
    : AbstractValidator<SubmitOrderCommand>
{
    public SubmitOrderCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.ShippingAddress)
            .NotNull().WithMessage("Shipping address is required.");

        When(x => x.ShippingAddress is not null, () =>
        {
            RuleFor(x => x.ShippingAddress.Name)
                .NotEmpty().WithMessage("Shipping name is required.");

            RuleFor(x => x.ShippingAddress.Email)
                .NotEmpty().WithMessage("Shipping email is required.")
                .EmailAddress().WithMessage("A valid shipping email is required.");

            RuleFor(x => x.ShippingAddress.Street)
                .NotEmpty().WithMessage("Street address is required.");

            RuleFor(x => x.ShippingAddress.City)
                .NotEmpty().WithMessage("City is required.");

            RuleFor(x => x.ShippingAddress.State)
                .NotEmpty().WithMessage("State is required.");

            RuleFor(x => x.ShippingAddress.ZipCode)
                .NotEmpty().WithMessage("Zip code is required.")
                .MaximumLength(10).WithMessage("Zip code must not exceed 10 characters.");
        });

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must contain at least one item.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductId)
                .NotEmpty().WithMessage("Product ID is required.");

            item.RuleFor(i => i.ProductName)
                .NotEmpty().WithMessage("Product name is required.");

            item.RuleFor(i => i.UnitPrice)
                .GreaterThan(0).WithMessage("Unit price must be greater than zero.");

            item.RuleFor(i => i.Quantity)
                .InclusiveBetween(1, 99).WithMessage("Quantity must be between 1 and 99.");
        });
    }
}
