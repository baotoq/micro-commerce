using MicroCommerce.ApiService.Features.Products;

namespace MicroCommerce.UnitTests.Features.Products;

public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _validator = new();

    [Fact]
    public void Valid_request_passes()
    {
        var result = _validator.Validate(new CreateProductRequest("Coffee Mug", 12.5m));

        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_name_fails(string name)
    {
        var result = _validator.Validate(new CreateProductRequest(name, 1m));

        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateProductRequest.Name));
    }

    [Fact]
    public void Name_longer_than_200_characters_fails()
    {
        var result = _validator.Validate(new CreateProductRequest(new string('a', 201), 1m));

        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateProductRequest.Name));
    }

    [Fact]
    public void Negative_price_fails()
    {
        var result = _validator.Validate(new CreateProductRequest("Coffee Mug", -0.01m));

        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateProductRequest.Price));
    }
}
