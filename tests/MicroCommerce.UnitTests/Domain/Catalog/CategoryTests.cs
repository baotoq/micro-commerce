using MicroCommerce.Domain.Catalog;
using MicroCommerce.Domain.SharedKernel;

namespace MicroCommerce.UnitTests.Domain.Catalog;

public class CategoryTests
{
    [Fact]
    public void A_top_level_category_has_no_parent_and_a_trimmed_name()
    {
        var id = CategoryId.New();

        var category = Category.CreateTopLevel(id, "  Home  ");

        category.Id.ShouldBe(id);
        category.Name.ShouldBe("Home");
        category.NormalizedName.ShouldBe("HOME");
        category.ParentId.ShouldBeNull();
    }

    [Fact]
    public void A_child_category_records_its_parent()
    {
        var parent = CategoryId.New();

        var child = Category.CreateChild(CategoryId.New(), "Kitchen", parent);

        child.ParentId.ShouldBe(parent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Refuses_a_blank_name(string name) =>
        Should.Throw<DomainRuleViolation>(() => Category.CreateTopLevel(CategoryId.New(), name));

    [Fact]
    public void Accepts_100_characters_and_refuses_101()
    {
        Category.CreateTopLevel(CategoryId.New(), new string('a', 100)).Name.Length.ShouldBe(100);
        Should.Throw<DomainRuleViolation>(() => Category.CreateTopLevel(CategoryId.New(), new string('a', 101)));
    }

    [Fact]
    public void Rename_changes_the_name_and_its_normalized_form_but_not_the_parent()
    {
        var parent = CategoryId.New();
        var category = Category.CreateChild(CategoryId.New(), "Kitchen", parent);

        category.Rename(" Cookware ");

        category.Name.ShouldBe("Cookware");
        category.NormalizedName.ShouldBe("COOKWARE");
        category.ParentId.ShouldBe(parent);
    }

    [Fact]
    public void Rename_refuses_a_blank_name() =>
        Should.Throw<DomainRuleViolation>(() => Category.CreateTopLevel(CategoryId.New(), "Home").Rename(" "));
}
