using Vogen;

namespace MicroCommerce.ApiService.Features.Coupons.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
public partial record struct CouponUsageId
{
    public static Validation Validate(Guid value) =>
        value != Guid.Empty
            ? Validation.Ok
            : Validation.Invalid("CouponUsageId cannot be empty.");

    public static CouponUsageId New() => From(Guid.CreateVersion7());
}
