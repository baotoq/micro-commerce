using MicroCommerce.ApiService.Features.Diagnostics;

namespace MicroCommerce.UnitTests.Features.Diagnostics;

public class SampleProcessCompletedCloudEventValidatorTests
{
    private readonly SampleProcessCompletedCloudEventValidator _validator = new();

    [Fact]
    public void Complete_event_is_valid()
    {
        var id = Guid.NewGuid();
        var data = new SampleProcessCompletedV1(SampleProcessCompletedV1.EventIdFor(id), id, DateTimeOffset.UtcNow);

        _validator.Validate(new SampleProcessCompletedCloudEvent(data)).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void Missing_data_is_invalid()
    {
        var result = _validator.Validate(new SampleProcessCompletedCloudEvent(null));

        result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe("Data");
    }

    [Theory]
    [InlineData("", "0193f1c2-0000-7000-8000-000000000001", "Data.EventId")]
    [InlineData("sample-process-completed:x", "00000000-0000-0000-0000-000000000000", "Data.SampleProcessId")]
    public void Event_id_and_process_id_are_required(string eventId, string sampleProcessId, string invalidProperty)
    {
        var data = new SampleProcessCompletedV1(eventId, Guid.Parse(sampleProcessId), DateTimeOffset.UtcNow);

        var result = _validator.Validate(new SampleProcessCompletedCloudEvent(data));

        result.Errors.ShouldHaveSingleItem().PropertyName.ShouldBe(invalidProperty);
    }
}
