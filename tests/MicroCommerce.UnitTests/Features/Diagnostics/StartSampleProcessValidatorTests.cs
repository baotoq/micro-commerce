using MicroCommerce.ApiService.Features.Diagnostics;

namespace MicroCommerce.UnitTests.Features.Diagnostics;

public class StartSampleProcessValidatorTests
{
    private readonly StartSampleProcessValidator _validator = new();

    [Theory]
    [InlineData(1)]
    [InlineData(300)]
    public void Delay_from_1_to_300_seconds_is_valid(int seconds)
    {
        _validator.Validate(new StartSampleProcessRequest(seconds)).IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(301)]
    [InlineData(null)]
    public void Missing_or_out_of_range_delay_is_invalid(int? seconds)
    {
        var result = _validator.Validate(new StartSampleProcessRequest(seconds));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldAllBe(e => e.PropertyName == nameof(StartSampleProcessRequest.DelaySeconds));
    }
}
