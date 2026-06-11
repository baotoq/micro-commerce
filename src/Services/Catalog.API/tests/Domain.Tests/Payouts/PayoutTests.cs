using MicroCommerce.Catalog.Domain.Payouts;

namespace MicroCommerce.Catalog.Domain.Tests.Payouts;

public class PayoutTests
{
    private static readonly DateTimeOffset SentAt =
        new(2026, 4, 1, 0, 0, 0, TimeSpan.FromHours(-7));

    private static Payout CreatePayout(
        decimal amount = 247.20m,
        string period = "Mar 12 – Apr 8",
        string destination = "Chase ···· 8847",
        bool isPending = true) =>
        new(SentAt, amount, period, destination, isPending);

    [Fact]
    public void Payout_Valid_Creates()
    {
        var payout = CreatePayout();

        Assert.NotEqual(default, payout.Id.Value);
        Assert.Equal(SentAt, payout.SentAt);
        Assert.Equal(247.20m, payout.Amount);
        Assert.Equal("Mar 12 – Apr 8", payout.Period);
        Assert.Equal("Chase ···· 8847", payout.Destination);
        Assert.True(payout.IsPending);
    }

    [Fact]
    public void Payout_ZeroAmount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreatePayout(amount: 0m));
    }

    [Fact]
    public void Payout_NegativeAmount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreatePayout(amount: -1m));
    }

    [Fact]
    public void Payout_EmptyPeriod_Throws()
    {
        Assert.Throws<ArgumentException>(() => CreatePayout(period: ""));
    }

    [Fact]
    public void Payout_WhitespacePeriod_Throws()
    {
        Assert.Throws<ArgumentException>(() => CreatePayout(period: "   "));
    }

    [Fact]
    public void Payout_EmptyDestination_Throws()
    {
        Assert.Throws<ArgumentException>(() => CreatePayout(destination: ""));
    }

    [Fact]
    public void Payout_WhitespaceDestination_Throws()
    {
        Assert.Throws<ArgumentException>(() => CreatePayout(destination: "   "));
    }

    [Fact]
    public void MarkSent_SetsPendingFalse()
    {
        var payout = CreatePayout(isPending: true);

        payout.MarkSent();

        Assert.False(payout.IsPending);
    }
}
