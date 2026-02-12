using FluentValidation;

namespace MicroCommerce.ApiService.Features.Ordering.Application.Commands.UpdateOrderStatus;

public sealed class UpdateOrderStatusCommandValidator
    : AbstractValidator<UpdateOrderStatusCommand>
{
    private static readonly string[] AllowedStatuses = ["Shipped", "Delivered"];

    public UpdateOrderStatusCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.");

        RuleFor(x => x.NewStatus)
            .NotEmpty().WithMessage("New status is required.")
            .Must(status => AllowedStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
            .WithMessage("New status must be one of: Shipped, Delivered.");
    }
}
