using MicroCommerce.ApiService.Infrastructure;
using MicroCommerce.Domain.SharedKernel;
using Microsoft.AspNetCore.Http;

namespace MicroCommerce.UnitTests.Infrastructure;

public class DomainRuleViolationHandlerTests
{
    private readonly IProblemDetailsService _problemDetails = Substitute.For<IProblemDetailsService>();

    [Fact]
    public async Task Domain_rule_violation_becomes_a_422_problem_with_its_message()
    {
        _problemDetails.TryWriteAsync(Arg.Any<ProblemDetailsContext>()).Returns(true);
        var context = new DefaultHttpContext();

        var handled = await new DomainRuleViolationHandler(_problemDetails)
            .TryHandleAsync(context, new DomainRuleViolation("A sample process must wait between 1 and 300 seconds"), TestContext.Current.CancellationToken);

        handled.ShouldBeTrue();
        context.Response.StatusCode.ShouldBe(StatusCodes.Status422UnprocessableEntity);
        await _problemDetails.Received(1).TryWriteAsync(Arg.Is<ProblemDetailsContext>(c =>
            c.ProblemDetails.Status == 422 && c.ProblemDetails.Detail == "A sample process must wait between 1 and 300 seconds"));
    }

    [Fact]
    public async Task Other_exceptions_are_not_handled()
    {
        var context = new DefaultHttpContext();

        var handled = await new DomainRuleViolationHandler(_problemDetails)
            .TryHandleAsync(context, new InvalidOperationException("boom"), TestContext.Current.CancellationToken);

        handled.ShouldBeFalse();
        await _problemDetails.DidNotReceive().TryWriteAsync(Arg.Any<ProblemDetailsContext>());
    }
}
