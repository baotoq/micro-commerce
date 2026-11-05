namespace MicroCommerce.Domain.SharedKernel;

/// <summary>
/// Thrown when an aggregate's invariant would be broken. The message is written in domain terms,
/// because the API returns it to the client as the problem detail.
/// </summary>
public class DomainRuleViolation(string message) : Exception(message);
