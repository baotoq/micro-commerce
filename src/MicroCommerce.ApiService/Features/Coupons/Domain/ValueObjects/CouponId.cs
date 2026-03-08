using Vogen;

namespace MicroCommerce.ApiService.Features.Coupons.Domain.ValueObjects;

[ValueObject<Guid>(conversions: Conversions.EfCoreValueConverter | Conversions.SystemTextJson)]
public partial record struct CouponId
{
    public static Validation Validate(Guid value) =>
        value != Guid.Empty
            ? Validation.Ok
            : Validation.Invalid("CouponId cannot be empty.");

    public static CouponId New() => From(Guid.CreateVersion7());
}
