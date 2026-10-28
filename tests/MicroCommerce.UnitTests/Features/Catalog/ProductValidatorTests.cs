using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.UnitTests.Features.Catalog;

public class ProductValidatorTests
{
    private readonly CreateProductValidator _create = new();
    private readonly UpdateProductValidator _update = new();

    private static readonly CreateProductRequest Valid = new("Coffee Mug", "Stoneware.", CategoryId.New(), 120_000, 5, 350);

    private IEnumerable<string> Failures(CreateProductRequest request)
    {
        var update = new UpdateProductRequest(
            ProductId.New(), request.Name, request.Description, request.CategoryId, request.Price, request.Stock, request.WeightGrams);
        var createFailures = _create.Validate(request).Errors.Select(e => e.PropertyName).ToList();

        // Creating and editing share their rules.
        _update.Validate(update).Errors.Select(e => e.PropertyName).ShouldBe(createFailures);
        return createFailures;
    }

    [Fact]
    public void Valid_request_passes()
    {
        Failures(Valid).ShouldBeEmpty();
    }

    [Fact]
    public void Description_is_optional_and_stock_may_be_zero()
    {
        Failures(Valid with { Description = null, Stock = 0 }).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_name_fails(string name)
    {
        Failures(Valid with { Name = name }).ShouldBe([nameof(CreateProductRequest.Name)]);
    }

    [Fact]
    public void Name_of_200_characters_passes_and_201_fails()
    {
        Failures(Valid with { Name = new string('a', 200) }).ShouldBeEmpty();
        Failures(Valid with { Name = new string('a', 201) }).ShouldBe([nameof(CreateProductRequest.Name)]);
    }

    [Fact]
    public void Description_over_5000_characters_fails()
    {
        Failures(Valid with { Description = new string('a', 5001) }).ShouldBe([nameof(CreateProductRequest.Description)]);
    }

    [Fact]
    public void Missing_category_fails()
    {
        Failures(Valid with { CategoryId = null }).ShouldBe([nameof(CreateProductRequest.CategoryId)]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Non_positive_price_fails(long price)
    {
        Failures(Valid with { Price = price }).ShouldBe([nameof(CreateProductRequest.Price)]);
    }

    [Fact]
    public void Negative_stock_fails()
    {
        Failures(Valid with { Stock = -1 }).ShouldBe([nameof(CreateProductRequest.Stock)]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Non_positive_weight_fails(int weight)
    {
        Failures(Valid with { WeightGrams = weight }).ShouldBe([nameof(CreateProductRequest.WeightGrams)]);
    }
}
