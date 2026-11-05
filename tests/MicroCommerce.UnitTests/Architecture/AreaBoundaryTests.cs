using System.Reflection;
using MicroCommerce.ApiService.Features.Accounts.Data;
using MicroCommerce.ApiService.Features.Catalog.Data;

namespace MicroCommerce.UnitTests.Architecture;

/// <summary>
/// Each area owns its data (Principle II). Catalog learns about Merchants only through the published
/// IMerchantDirectory contract, never through the Accounts DbContext, and the reverse (research R1).
/// Both areas share one assembly, so this checks constructor parameters, fields and properties.
/// </summary>
public class AreaBoundaryTests
{
    private static readonly Assembly Api = typeof(CatalogDbContext).Assembly;

    [Fact]
    public void Catalog_never_uses_the_accounts_context() =>
        Offenders("MicroCommerce.ApiService.Features.Catalog", typeof(AccountsDbContext)).ShouldBeEmpty();

    [Fact]
    public void Accounts_never_uses_the_catalog_context() =>
        Offenders("MicroCommerce.ApiService.Features.Accounts", typeof(CatalogDbContext)).ShouldBeEmpty();

    private static List<string> Offenders(string area, Type forbidden)
    {
        const BindingFlags all = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        return Api.GetTypes()
            .Where(t => t.Namespace?.StartsWith(area, StringComparison.Ordinal) == true)
            .Where(t => t.GetConstructors(all).SelectMany(c => c.GetParameters()).Any(p => p.ParameterType == forbidden)
                || t.GetFields(all).Any(f => f.FieldType == forbidden)
                || t.GetProperties(all).Any(p => p.PropertyType == forbidden))
            .Select(t => t.FullName!)
            .ToList();
    }
}
