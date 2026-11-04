using MicroCommerce.ApiService.Infrastructure.Mapping;

namespace MicroCommerce.UnitTests.Infrastructure;

public class MappingConfigTests
{
    /// <summary>Every registered mapping is complete and explicit, checked here instead of at request time (research R3).</summary>
    [Fact]
    public void Mapping_configuration_compiles()
    {
        Should.NotThrow(() => MappingConfig.Create().Compile());
    }
}
