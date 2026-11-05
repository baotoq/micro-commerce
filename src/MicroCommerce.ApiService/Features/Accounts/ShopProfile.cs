using FluentValidation;
using MicroCommerce.Domain.Accounts;

namespace MicroCommerce.ApiService.Features.Accounts;

public sealed record PickupAddressDto(string Street, string District, string Province);

/// <summary>The owner's view of its Shop, Pickup Address included.</summary>
public sealed record ShopProfileResponse(Guid MerchantId, string ShopName, string? Description, PickupAddressDto PickupAddress, int Version)
{
    public static ShopProfileResponse From(Merchant m) => new(
        m.Id.Value,
        m.ShopName.Value,
        m.Description,
        new PickupAddressDto(m.PickupAddress.Street, m.PickupAddress.District, m.PickupAddress.Province),
        m.Version);
}

/// <summary>The Shop profile rules shared by opening a Merchant and editing its Shop.</summary>
internal static class ShopProfileRules
{
    public static void AddRules<T>(AbstractValidator<T> v, Func<T, string?> shopName, Func<T, string?> description, Func<T, PickupAddressDto?> address)
    {
        v.RuleFor(x => shopName(x)).Must(n => n?.Trim().Length is >= ShopName.MinLength and <= ShopName.MaxLength)
            .WithMessage($"shopName must be {ShopName.MinLength} to {ShopName.MaxLength} characters.")
            .OverridePropertyName("ShopName");
        v.RuleFor(x => description(x)).MaximumLength(Merchant.MaxDescriptionLength)
            .WithMessage($"description must be at most {Merchant.MaxDescriptionLength} characters.")
            .OverridePropertyName("Description");
        v.RuleFor(x => address(x)).NotNull().WithMessage("pickupAddress is required.").OverridePropertyName("PickupAddress");
        v.When(x => address(x) is not null, () =>
        {
            v.RuleFor(x => address(x)!.Street).Must(s => Valid(s, PickupAddress.MaxStreetLength))
                .WithMessage($"street is required and at most {PickupAddress.MaxStreetLength} characters.")
                .OverridePropertyName("PickupAddress.Street");
            v.RuleFor(x => address(x)!.District).Must(s => Valid(s, PickupAddress.MaxDistrictLength))
                .WithMessage($"district is required and at most {PickupAddress.MaxDistrictLength} characters.")
                .OverridePropertyName("PickupAddress.District");
            v.RuleFor(x => address(x)!.Province).Must(s => Valid(s, PickupAddress.MaxProvinceLength))
                .WithMessage($"province is required and at most {PickupAddress.MaxProvinceLength} characters.")
                .OverridePropertyName("PickupAddress.Province");
        });
    }

    private static bool Valid(string? value, int maxLength) => value?.Trim().Length is > 0 && value.Trim().Length <= maxLength;

    public static PickupAddress ToDomain(this PickupAddressDto dto) => new(dto.Street, dto.District, dto.Province);
}
