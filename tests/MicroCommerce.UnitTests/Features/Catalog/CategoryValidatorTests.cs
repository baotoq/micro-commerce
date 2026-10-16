using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.UnitTests.Features.Catalog;

public class CategoryValidatorTests
{
    private readonly CreateCategoryValidator _create = new();
    private readonly RenameCategoryValidator _rename = new();

    [Fact]
    public void Top_level_and_child_categories_pass()
    {
        _create.Validate(new CreateCategoryRequest("Fashion", null)).IsValid.ShouldBeTrue();
        _create.Validate(new CreateCategoryRequest("Men", CategoryId.New())).IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Empty_name_fails(string name)
    {
        _create.Validate(new CreateCategoryRequest(name, null)).Errors
            .Select(e => e.PropertyName).ShouldBe([nameof(CreateCategoryRequest.Name)]);
        _rename.Validate(new RenameCategoryRequest(CategoryId.New(), name)).Errors
            .Select(e => e.PropertyName).ShouldBe([nameof(RenameCategoryRequest.Name)]);
    }

    [Fact]
    public void Name_of_100_characters_passes_and_101_fails()
    {
        _create.Validate(new CreateCategoryRequest(new string('a', 100), null)).IsValid.ShouldBeTrue();
        _create.Validate(new CreateCategoryRequest(new string('a', 101), null)).IsValid.ShouldBeFalse();
        _rename.Validate(new RenameCategoryRequest(CategoryId.New(), new string('a', 101))).IsValid.ShouldBeFalse();
    }
}
