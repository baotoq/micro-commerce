using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace MicroCommerce.Shared.Common;

public interface IEndpointDefinition
{
    void DefineEnpoints(WebApplication app);
    void DefineServices(IServiceCollection services);
}
