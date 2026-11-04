using MicroCommerce.Domain.Diagnostics;
using MicroCommerce.Domain.SharedKernel;

namespace MicroCommerce.UnitTests.Domain;

public class SampleProcessTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData(1)]
    [InlineData(300)]
    public void Starts_with_a_delay_from_1_to_300_seconds(int seconds)
    {
        var id = SampleProcessId.New();

        var process = SampleProcess.Start(id, TimeSpan.FromSeconds(seconds), Now);

        process.Id.ShouldBe(id);
        process.Delay.ShouldBe(TimeSpan.FromSeconds(seconds));
        process.Status.ShouldBe(SampleProcessStatus.Running);
        process.StartedAt.ShouldBe(Now);
        process.CompletedAt.ShouldBeNull();
        process.DomainEvents.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(301)]
    public void Refuses_a_delay_outside_1_to_300_seconds(int seconds)
    {
        var violation = Should.Throw<DomainRuleViolation>(() =>
            SampleProcess.Start(SampleProcessId.New(), TimeSpan.FromSeconds(seconds), Now));

        violation.Message.ShouldBe("A sample process must wait between 1 and 300 seconds");
    }

    [Fact]
    public void Cannot_complete_before_its_delay_has_passed()
    {
        var process = SampleProcess.Start(SampleProcessId.New(), TimeSpan.FromSeconds(10), Now);

        Should.Throw<DomainRuleViolation>(() => process.Complete(Now.AddSeconds(9)));

        process.Status.ShouldBe(SampleProcessStatus.Running);
        process.DomainEvents.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(10)]
    [InlineData(11)]
    public void Completes_once_its_delay_has_passed(int secondsLater)
    {
        var process = SampleProcess.Start(SampleProcessId.New(), TimeSpan.FromSeconds(10), Now);
        var completedAt = Now.AddSeconds(secondsLater);

        process.Complete(completedAt);

        process.Status.ShouldBe(SampleProcessStatus.Completed);
        process.CompletedAt.ShouldBe(completedAt);
        process.DomainEvents.ShouldHaveSingleItem().ShouldBe(new SampleProcessCompleted(process.Id, completedAt));
    }

    [Fact]
    public void Completing_again_changes_nothing()
    {
        var process = SampleProcess.Start(SampleProcessId.New(), TimeSpan.FromSeconds(1), Now);
        process.Complete(Now.AddSeconds(1));

        process.Complete(Now.AddSeconds(5));

        process.CompletedAt.ShouldBe(Now.AddSeconds(1));
        process.DomainEvents.Count.ShouldBe(1);
    }
}
