using MicroCommerce.Domain.SharedKernel;

namespace MicroCommerce.Domain.Diagnostics;

public sealed record SampleProcessCompleted(SampleProcessId Id, DateTimeOffset OccurredAt) : IDomainEvent;
