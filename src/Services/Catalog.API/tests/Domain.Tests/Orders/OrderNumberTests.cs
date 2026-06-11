using MicroCommerce.Catalog.Domain.Orders;
using Vogen;

namespace MicroCommerce.Catalog.Domain.Tests.Orders;

public class OrderNumberTests
{
    [Fact]
    public void From_Zero_Throws()
    {
        Assert.Throws<ValueObjectValidationException>(() => OrderNumber.From(0));
    }

    [Fact]
    public void From_Negative_Throws()
    {
        Assert.Throws<ValueObjectValidationException>(() => OrderNumber.From(-1));
    }

    [Fact]
    public void From_Positive_Succeeds()
    {
        var number = OrderNumber.From(1042);
        Assert.Equal(1042, number.Value);
    }
}
