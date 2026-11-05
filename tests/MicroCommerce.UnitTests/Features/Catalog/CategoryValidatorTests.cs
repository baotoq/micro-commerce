using MicroCommerce.ApiService.Features.Catalog.Categories;

namespace MicroCommerce.UnitTests.Features.Catalog;

public class CategoryValidatorTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void A_name_is_required(string? name)
    {
        new CreateCategoryValidator().Validate(new CreateCategoryRequest(name!, null)).Errors.Select(e => e.PropertyName).ShouldBe(["Name"]);
        new RenameCategoryValidator().Validate(new RenameCategoryRequest(Guid.NewGuid(), name!)).Errors.Select(e => e.PropertyName).ShouldBe(["Name"]);
    }

    [Fact]
    public void A_name_is_at_most_100_characters()
    {
        new CreateCategoryValidator().Validate(new CreateCategoryRequest(new string('n', 100), null)).IsValid.ShouldBeTrue();
        new CreateCategoryValidator().Validate(new CreateCategoryRequest(new string('n', 101), null)).IsValid.ShouldBeFalse();
        new RenameCategoryValidator().Validate(new RenameCategoryRequest(Guid.NewGuid(), new string('n', 101))).IsValid.ShouldBeFalse();
    }
}
