using FluentAssertions;
using MicroCommerce.ApiService.Features.Catalog.Domain.ValueObjects;

namespace MicroCommerce.ApiService.Tests.Unit.Catalog.ValueObjects;

[Trait("Category", "Unit")]
public sealed class MoneyTests
{
    [Fact]
    public void Create_ValidAmount_ReturnsMoney()
    {
        // Arrange & Act
        Money money = Money.Create(99.99m, "USD");

        // Assert
        money.Amount.Should().Be(99.99m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_ZeroAmount_ReturnsMoney()
    {
        // Arrange & Act
        Money money = Money.Create(0m, "USD");

        // Assert
        money.Amount.Should().Be(0m);
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Create_NegativeAmount_ThrowsException()
    {
        // Arrange & Act
        Action act = () => Money.Create(-10.00m, "USD");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("amount");
    }

    [Fact]
    public void Create_NullCurrency_ThrowsException()
    {
        // Arrange & Act
        Action act = () => Money.Create(50.00m, null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("currency");
    }

    [Fact]
    public void Create_LowercaseCurrency_NormalizesToUppercase()
    {
        // Arrange & Act
        Money money = Money.Create(100.00m, "usd");

        // Assert
        money.Currency.Should().Be("USD");
    }

    [Fact]
    public void Equality_SameAmountAndCurrency_AreEqual()
    {
        // Arrange
        Money money1 = Money.Create(99.99m, "USD");
        Money money2 = Money.Create(99.99m, "USD");

        // Act & Assert
        money1.Should().Be(money2);
        (money1 == money2).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentAmount_AreNotEqual()
    {
        // Arrange
        Money money1 = Money.Create(99.99m, "USD");
        Money money2 = Money.Create(50.00m, "USD");

        // Act & Assert
        money1.Should().NotBe(money2);
        (money1 != money2).Should().BeTrue();
    }

    [Fact]
    public void Equality_DifferentCurrency_AreNotEqual()
    {
        // Arrange
        Money money1 = Money.Create(99.99m, "USD");
        Money money2 = Money.Create(99.99m, "EUR");

        // Act & Assert
        money1.Should().NotBe(money2);
    }

    [Fact]
    public void ToString_ReturnsFormattedString()
    {
        // Arrange
        Money money = Money.Create(99.99m, "USD");

        // Act
        string result = money.ToString();

        // Assert
        result.Should().MatchRegex(@"USD 99[.,]99");
    }

    [Fact]
    public void Format_ReturnsCurrencyFormat()
    {
        // Arrange
        Money money = Money.Create(99.99m, "USD");

        // Act
        string result = money.Format();

        // Assert
        result.Should().Be("$99.99");
    }
}
