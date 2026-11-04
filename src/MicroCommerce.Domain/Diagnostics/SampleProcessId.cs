using Vogen;

namespace MicroCommerce.Domain.Diagnostics;

[ValueObject<Guid>]
public readonly partial struct SampleProcessId
{
    public static SampleProcessId New() => From(Guid.CreateVersion7());
}
