using MicroCommerce.Domain.SharedKernel;

namespace MicroCommerce.UnitTests.Architecture;

/// <summary>The domain depends on no persistence, transport, messaging, workflow or web framework (FR-013).</summary>
public class DomainDependencyTests
{
    private static readonly string[] Forbidden =
    [
        "Microsoft.EntityFrameworkCore", "Microsoft.AspNetCore", "FastEndpoints", "Temporalio", "Dapr", "Mapster", "Npgsql",
    ];

    [Fact]
    public void Domain_references_only_the_bcl_and_vogen()
    {
        var references = typeof(AggregateRoot<>).Assembly.GetReferencedAssemblies().Select(a => a.Name!).ToList();

        references.ShouldNotContain(name => Forbidden.Any(f => name.StartsWith(f, StringComparison.Ordinal)));
        references.ShouldAllBe(name => name.StartsWith("System") || name == "netstandard" || name.StartsWith("Vogen"));
    }
}
