using MicroCommerce.ApiService.Features.Ordering;
using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.UnitTests.Features.Ordering;

public class CartValidatorTests
{
    private readonly AddCartLineValidator _addValidator = new();
    private readonly UpdateCartLineValidator _updateValidator = new();
    private readonly CartValidator _cartValidator = new();

    [Theory]
    [InlineData(1)]
    [InlineData(99)]
    public void Quantity_from_1_to_99_passes(int quantity)
    {
        _addValidator.Validate(new AddCartLineRequest(VariantId.New(), quantity)).IsValid.ShouldBeTrue();
        _updateValidator.Validate(new UpdateCartLineRequest(VariantId.New(), quantity)).IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(100)]
    public void Quantity_outside_1_to_99_fails(int quantity)
    {
        _addValidator.Validate(new AddCartLineRequest(VariantId.New(), quantity))
            .Errors.Select(e => e.PropertyName).ShouldBe([nameof(AddCartLineRequest.Quantity)]);
        _updateValidator.Validate(new UpdateCartLineRequest(VariantId.New(), quantity))
            .Errors.Select(e => e.PropertyName).ShouldBe([nameof(UpdateCartLineRequest.Quantity)]);
    }

    [Fact]
    public void Cart_with_50_lines_passes()
    {
        var cart = new Cart { AccountId = AccountId.New() };
        for (var i = 0; i < 50; i++) cart.Add(VariantId.New(), 1);

        _cartValidator.Validate(cart).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Cart_with_51_lines_fails()
    {
        var cart = new Cart { AccountId = AccountId.New() };
        for (var i = 0; i < 51; i++) cart.Add(VariantId.New(), 1);

        _cartValidator.Validate(cart).Errors.Select(e => e.PropertyName).ShouldBe([nameof(Cart.Lines)]);
    }

    [Fact]
    public void Adding_past_99_of_one_variant_fails()
    {
        var cart = new Cart { AccountId = AccountId.New() };
        var variantId = VariantId.New();
        cart.Add(variantId, 98);
        cart.Add(variantId, 2);

        cart.Lines.ShouldHaveSingleItem().Quantity.ShouldBe(100);
        _cartValidator.Validate(cart).IsValid.ShouldBeFalse();
    }
}
