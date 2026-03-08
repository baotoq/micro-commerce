using FluentValidation;
using MicroCommerce.ApiService.Features.Coupons.Domain.ValueObjects;

namespace MicroCommerce.ApiService.Features.Coupons.Application.Commands.UpdateCoupon;

public sealed class UpdateCouponCommandValidator : AbstractValidator<UpdateCouponCommand>
{
    public UpdateCouponCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.DiscountType)
            .NotEmpty().WithMessage("DiscountType is required.")
            .Must(t => DiscountType.TryFromName(t, ignoreCase: true, out _))
            .WithMessage("DiscountType must be 'Percentage' or 'FixedAmount'.");

        RuleFor(x => x.DiscountValue)
            .GreaterThan(0).WithMessage("DiscountValue must be greater than 0.");

        RuleFor(x => x.DiscountValue)
            .LessThanOrEqualTo(100)
            .When(x => DiscountType.TryFromName(x.DiscountType, ignoreCase: true, out DiscountType? dt)
                       && dt == DiscountType.Percentage)
            .WithMessage("DiscountValue for percentage discounts must not exceed 100.");

        RuleFor(x => x.ValidFrom)
            .NotEmpty().WithMessage("ValidFrom is required.");

        RuleFor(x => x.ValidUntil)
            .GreaterThan(x => x.ValidFrom)
            .When(x => x.ValidUntil.HasValue)
            .WithMessage("ValidUntil must be after ValidFrom.");

        RuleFor(x => x.MinOrderAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.MinOrderAmount.HasValue)
            .WithMessage("MinOrderAmount must be >= 0.");
    }
}
