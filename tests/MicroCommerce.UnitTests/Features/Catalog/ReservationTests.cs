using MicroCommerce.ApiService.Features.Catalog;
using MicroCommerce.ApiService.SharedKernel;

namespace MicroCommerce.UnitTests.Features.Catalog;

public class ReservationTests
{
    private static readonly DateTimeOffset Now = new(2026, 10, 29, 12, 0, 0, TimeSpan.Zero);

    private static Reservation NewReservation(DateTimeOffset? holdUntil = null) =>
        Reservation.Create(CheckoutId.New(), VariantId.New(), ProductId.New(), 2, holdUntil ?? Now.AddMinutes(30));

    private static bool IsActive(Reservation reservation, DateTimeOffset now) => Reservation.ActiveAt(now).Compile()(reservation);

    [Fact]
    public void New_reservation_is_reserved_until_its_deadline()
    {
        var reservation = NewReservation(Now.AddMinutes(30));

        reservation.State.ShouldBe(ReservationState.Reserved);
        reservation.HoldUntil.ShouldBe(Now.AddMinutes(30));
    }

    [Fact]
    public void Reserved_counts_until_its_deadline_and_not_after()
    {
        var reservation = NewReservation(Now.AddMinutes(30));

        IsActive(reservation, Now).ShouldBeTrue();
        IsActive(reservation, Now.AddMinutes(30)).ShouldBeFalse();
        IsActive(reservation, Now.AddHours(1)).ShouldBeFalse();
    }

    [Fact]
    public void Commit_makes_it_held_with_no_deadline_and_it_always_counts()
    {
        var reservation = NewReservation(Now.AddMinutes(30));

        reservation.Commit().ShouldBeTrue();

        reservation.State.ShouldBe(ReservationState.Held);
        reservation.HoldUntil.ShouldBeNull();
        IsActive(reservation, Now.AddYears(1)).ShouldBeTrue();
    }

    [Fact]
    public void Commit_also_takes_a_reservation_past_its_deadline()
    {
        var reservation = NewReservation(Now.AddMinutes(-1));

        reservation.Commit().ShouldBeTrue();

        reservation.State.ShouldBe(ReservationState.Held);
    }

    [Fact]
    public void Repeating_commit_changes_nothing()
    {
        var reservation = NewReservation();
        reservation.Commit();

        reservation.Commit().ShouldBeTrue();

        reservation.State.ShouldBe(ReservationState.Held);
    }

    [Fact]
    public void Release_frees_a_reserved_or_held_reservation()
    {
        var reserved = NewReservation();
        var held = NewReservation();
        held.Commit();

        reserved.Release();
        held.Release();

        reserved.State.ShouldBe(ReservationState.Released);
        held.State.ShouldBe(ReservationState.Released);
        IsActive(reserved, Now).ShouldBeFalse();
        IsActive(held, Now).ShouldBeFalse();
    }

    [Fact]
    public void Released_reservation_cannot_be_committed_and_releasing_it_again_changes_nothing()
    {
        var reservation = NewReservation();
        reservation.Release();

        reservation.Commit().ShouldBeFalse();
        reservation.Release();

        reservation.State.ShouldBe(ReservationState.Released);
    }

    [Theory]
    [InlineData(10, 0, 10)]
    [InlineData(10, 3, 7)]
    [InlineData(10, 10, 0)]
    [InlineData(2, 5, 0)]
    public void Available_is_stock_minus_reserved_never_below_zero(int stock, int reserved, int available)
    {
        Availability.Of(stock, reserved).ShouldBe(available);
    }
}
