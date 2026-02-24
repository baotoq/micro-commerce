using Vogen;

namespace MicroCommerce.BuildingBlocks.Common.Events;

[ValueObject<Guid>(conversions: Conversions.SystemTextJson)]
public partial record struct EventId
{
    public static Validation Validate(Guid value) =>
        value != Guid.Empty
            ? Validation.Ok
            : Validation.Invalid("EventId cannot be empty.");

    public static EventId New() => From(Guid.CreateVersion7());
}
