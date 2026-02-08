using FluentAssertions;
using MicroCommerce.ApiService.Features.Ordering.Domain.ValueObjects;

namespace MicroCommerce.ApiService.Tests.Unit.Ordering.ValueObjects;

[Trait("Category", "Unit")]
public sealed class OrderNumberTests
{
    [Fact]
    public void Generate_ReturnsValueStartingWithMC()
    {
        // Act
        OrderNumber orderNumber = OrderNumber.Generate();

        // Assert
        orderNumber.Value.Should().StartWith("MC-");
    }

    [Fact]
    public void Generate_ReturnsValueWithCorrectLength()
    {
        // Act
        OrderNumber orderNumber = OrderNumber.Generate();

        // Assert
        orderNumber.Value.Should().HaveLength(9); // "MC-" + 6 characters
    }

    [Fact]
    public void Generate_TwoCallsReturnDifferentValues()
    {
        // Act
        OrderNumber orderNumber1 = OrderNumber.Generate();
        OrderNumber orderNumber2 = OrderNumber.Generate();

        // Assert
        orderNumber1.Value.Should().NotBe(orderNumber2.Value);
    }

    [Fact]
    public void Generate_UsesOnlyUnambiguousCharacters()
    {
        // Arrange
        string ambiguousChars = "0O1IL";

        // Act - generate multiple order numbers to increase confidence
        List<OrderNumber> orderNumbers = [];
        for (int i = 0; i < 100; i++)
        {
            orderNumbers.Add(OrderNumber.Generate());
        }

        // Assert
        foreach (OrderNumber orderNumber in orderNumbers)
        {
            string code = orderNumber.Value[3..]; // Skip "MC-" prefix
            code.Should().NotContainAny(ambiguousChars);
        }
    }

    [Fact]
    public void From_ValidValue_ReturnsOrderNumber()
    {
        // Arrange
        string validValue = "MC-ABC123";

        // Act
        OrderNumber orderNumber = OrderNumber.From(validValue);

        // Assert
        orderNumber.Value.Should().Be(validValue);
    }

    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        string expectedValue = "MC-XYZ789";
        OrderNumber orderNumber = OrderNumber.From(expectedValue);

        // Act
        string result = orderNumber.ToString();

        // Assert
        result.Should().Be(expectedValue);
    }
}
