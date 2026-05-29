using MicroCommerce.Catalog.Domain.Promotions;
using Vogen;

namespace MicroCommerce.Catalog.Domain.Tests.Promotions;

public class PromotionTests
{
    private static PromotionCode ValidCode() => PromotionCode.From("SPRING20");

    private static Promotion CreatePercentage(
        string description = "sitewide",
        int percentValue = 20,
        decimal? minOrderAmount = null,
        DateTimeOffset? startsAt = null,
        DateTimeOffset? endsAt = null) =>
        new(
            ValidCode(),
            description,
            DiscountKind.Percentage,
            percentValue,
            fixedAmount: null,
            minOrderAmount,
            startsAt,
            endsAt);

    private static Promotion CreateFixed(
        string description = "first order",
        decimal fixedAmount = 10m,
        decimal? minOrderAmount = null,
        DateTimeOffset? startsAt = null,
        DateTimeOffset? endsAt = null) =>
        new(
            ValidCode(),
            description,
            DiscountKind.FixedAmount,
            percentValue: null,
            fixedAmount,
            minOrderAmount,
            startsAt,
            endsAt);

    [Fact]
    public void Promotion_ValidPercentage_CreatesAsDraft()
    {
        var promo = CreatePercentage();
        Assert.NotEqual(default, promo.Id.Value);
        Assert.Equal("SPRING20", promo.Code.Value);
        Assert.Equal(PromotionStatus.Draft, promo.Status);
        Assert.Equal(DiscountKind.Percentage, promo.Kind);
        Assert.Equal(20, promo.PercentValue);
        Assert.Null(promo.FixedAmount);
    }

    [Fact]
    public void Promotion_ValidFixed_CreatesAsDraft()
    {
        var promo = CreateFixed();
        Assert.Equal(PromotionStatus.Draft, promo.Status);
        Assert.Equal(DiscountKind.FixedAmount, promo.Kind);
        Assert.Equal(10m, promo.FixedAmount);
        Assert.Null(promo.PercentValue);
    }

    [Fact]
    public void Promotion_EmptyDescription_Throws()
    {
        Assert.Throws<ArgumentException>(() => CreatePercentage(description: ""));
    }

    [Fact]
    public void Promotion_WhitespaceDescription_Throws()
    {
        Assert.Throws<ArgumentException>(() => CreatePercentage(description: "   "));
    }

    [Fact]
    public void Promotion_DescriptionTooLong_Throws()
    {
        var longDescription = new string('a', 201);
        Assert.Throws<ArgumentException>(() => CreatePercentage(description: longDescription));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void Promotion_PercentValueOutOfRange_Throws(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreatePercentage(percentValue: value));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Promotion_FixedAmountNonPositive_Throws(decimal value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateFixed(fixedAmount: value));
    }

    [Fact]
    public void Promotion_PercentageWithFixedAmount_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new Promotion(
                ValidCode(),
                "sitewide",
                DiscountKind.Percentage,
                percentValue: 20,
                fixedAmount: 5m,
                minOrderAmount: null,
                startsAt: null,
                endsAt: null));
    }

    [Fact]
    public void Promotion_PercentageWithoutValue_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new Promotion(
                ValidCode(),
                "sitewide",
                DiscountKind.Percentage,
                percentValue: null,
                fixedAmount: null,
                minOrderAmount: null,
                startsAt: null,
                endsAt: null));
    }

    [Fact]
    public void Promotion_FixedWithPercentValue_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new Promotion(
                ValidCode(),
                "first order",
                DiscountKind.FixedAmount,
                percentValue: 20,
                fixedAmount: 10m,
                minOrderAmount: null,
                startsAt: null,
                endsAt: null));
    }

    [Fact]
    public void Promotion_FixedWithoutAmount_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new Promotion(
                ValidCode(),
                "first order",
                DiscountKind.FixedAmount,
                percentValue: null,
                fixedAmount: null,
                minOrderAmount: null,
                startsAt: null,
                endsAt: null));
    }

    [Fact]
    public void Promotion_NegativeMinOrderAmount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreatePercentage(minOrderAmount: -1m));
    }

    [Fact]
    public void Promotion_MinOrderAmountZero_IsAllowed()
    {
        var promo = CreatePercentage(minOrderAmount: 0m);
        Assert.Equal(0m, promo.MinOrderAmount);
    }

    [Fact]
    public void Promotion_StartsAtEqualsEndsAt_Throws()
    {
        var t = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        Assert.Throws<ArgumentException>(() => CreatePercentage(startsAt: t, endsAt: t));
    }

    [Fact]
    public void Promotion_StartsAtAfterEndsAt_Throws()
    {
        var starts = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);
        var ends = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        Assert.Throws<ArgumentException>(() => CreatePercentage(startsAt: starts, endsAt: ends));
    }

    [Fact]
    public void Promotion_OnlyStartsAtSet_IsAllowed()
    {
        var starts = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var promo = CreatePercentage(startsAt: starts);
        Assert.Equal(starts, promo.StartsAt);
        Assert.Null(promo.EndsAt);
    }

    [Fact]
    public void Promotion_OnlyEndsAtSet_IsAllowed()
    {
        var ends = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);
        var promo = CreatePercentage(endsAt: ends);
        Assert.Null(promo.StartsAt);
        Assert.Equal(ends, promo.EndsAt);
    }

    [Fact]
    public void Activate_FromDraft_TransitionsToActive()
    {
        var promo = CreatePercentage();
        promo.Activate();
        Assert.Equal(PromotionStatus.Active, promo.Status);
    }

    [Fact]
    public void Activate_FromActive_Throws()
    {
        var promo = CreatePercentage();
        promo.Activate();
        Assert.Throws<InvalidOperationException>(() => promo.Activate());
    }

    [Fact]
    public void Activate_FromEnded_Throws()
    {
        var promo = CreatePercentage();
        promo.Activate();
        promo.End();
        Assert.Throws<InvalidOperationException>(() => promo.Activate());
    }

    [Fact]
    public void End_FromActive_TransitionsToEnded()
    {
        var promo = CreatePercentage();
        promo.Activate();
        promo.End();
        Assert.Equal(PromotionStatus.Ended, promo.Status);
    }

    [Fact]
    public void End_FromDraft_Throws()
    {
        var promo = CreatePercentage();
        Assert.Throws<InvalidOperationException>(() => promo.End());
    }

    [Fact]
    public void End_FromEnded_Throws()
    {
        var promo = CreatePercentage();
        promo.Activate();
        promo.End();
        Assert.Throws<InvalidOperationException>(() => promo.End());
    }

    [Fact]
    public void UpdateDetails_FromDraft_AppliesChanges()
    {
        var promo = CreatePercentage();
        promo.UpdateDetails(
            description: "new sitewide",
            kind: DiscountKind.FixedAmount,
            percentValue: null,
            fixedAmount: 15m,
            minOrderAmount: 50m,
            startsAt: null,
            endsAt: null);

        Assert.Equal("new sitewide", promo.Description);
        Assert.Equal(DiscountKind.FixedAmount, promo.Kind);
        Assert.Null(promo.PercentValue);
        Assert.Equal(15m, promo.FixedAmount);
        Assert.Equal(50m, promo.MinOrderAmount);
    }

    [Fact]
    public void UpdateDetails_FromActive_AppliesChanges()
    {
        var promo = CreatePercentage();
        promo.Activate();
        promo.UpdateDetails(
            description: "updated",
            kind: DiscountKind.Percentage,
            percentValue: 30,
            fixedAmount: null,
            minOrderAmount: null,
            startsAt: null,
            endsAt: null);

        Assert.Equal("updated", promo.Description);
        Assert.Equal(30, promo.PercentValue);
        Assert.Equal(PromotionStatus.Active, promo.Status);
    }

    [Fact]
    public void UpdateDetails_FromEnded_Throws()
    {
        var promo = CreatePercentage();
        promo.Activate();
        promo.End();
        Assert.Throws<InvalidOperationException>(() => promo.UpdateDetails(
            description: "anything",
            kind: DiscountKind.Percentage,
            percentValue: 50,
            fixedAmount: null,
            minOrderAmount: null,
            startsAt: null,
            endsAt: null));
    }

    [Fact]
    public void UpdateDetails_InvalidInvariant_Throws()
    {
        var promo = CreatePercentage();
        Assert.Throws<ArgumentOutOfRangeException>(() => promo.UpdateDetails(
            description: "sitewide",
            kind: DiscountKind.Percentage,
            percentValue: 0,
            fixedAmount: null,
            minOrderAmount: null,
            startsAt: null,
            endsAt: null));
    }

    [Fact]
    public void PromotionCode_NormalizesTrimAndUppercase()
    {
        var code = PromotionCode.From("  spring20  ");
        Assert.Equal("SPRING20", code.Value);
    }

    [Fact]
    public void PromotionCode_Empty_Throws()
    {
        Assert.Throws<ValueObjectValidationException>(() => PromotionCode.From(""));
    }

    [Fact]
    public void PromotionCode_Whitespace_Throws()
    {
        Assert.Throws<ValueObjectValidationException>(() => PromotionCode.From("   "));
    }

    [Fact]
    public void PromotionCode_SameValueAreEqual()
    {
        var a = PromotionCode.From("spring20");
        var b = PromotionCode.From("SPRING20");
        Assert.Equal(a, b);
    }
}
