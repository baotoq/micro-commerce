using MicroCommerce.Domain.Products;
using Vogen;

namespace MicroCommerce.Domain.Tests.Products;

public class SkuTests
{
    [Fact]
    public void Sku_ValidValue_NormalizesToUppercase()
    {
        var sku = Sku.From("mc-vs-001");
        Assert.Equal("MC-VS-001", sku.Value);
    }

    [Fact]
    public void Sku_EmptyValue_Throws()
    {
        Assert.Throws<ValueObjectValidationException>(() => Sku.From(""));
    }

    [Fact]
    public void Sku_WhitespaceOnly_Throws()
    {
        Assert.Throws<ValueObjectValidationException>(() => Sku.From("   "));
    }

    [Fact]
    public void Sku_ConversionToString_ReturnsValue()
    {
        var sku = Sku.From("abc-123");
        string value = (string)sku;
        Assert.Equal("ABC-123", value);
    }

    [Fact]
    public void Sku_Equality_SameValue_AreEqual()
    {
        var sku1 = Sku.From("mc-001");
        var sku2 = Sku.From("MC-001");
        Assert.Equal(sku1, sku2);
    }
}
