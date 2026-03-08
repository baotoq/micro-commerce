using FluentValidation;
using MicroCommerce.ApiService.Features.Coupons.Domain.ValueObjects;

namespace MicroCommerce.ApiService.Features.Coupons.Application.Commands.CreateCoupon;

public sealed class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
{
    public CreateCouponCommandValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code must not exceed 50 characters.")
            .Matches(@"^[A-Za-z0-9\-]+$").WithMessage("Code may only contain letters, digits, and hyphens.");

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

        RuleFor(x => x.UsageLimit)
            .GreaterThan(0)
            .When(x => x.UsageLimit.HasValue)
            .WithMessage("UsageLimit must be greater than 0.");

        RuleFor(x => x.UsagePerUser)
            .GreaterThan(0)
            .When(x => x.UsagePerUser.HasValue)
            .WithMessage("UsagePerUser must be greater than 0.");
    }
}
