using System.Reflection;
using MicroCommerce.Shared;

namespace MicroCommerce.Basket.API.EndpointDefinitions;

public class SwaggerEndpoint : IEndpointDefinition
{
    public void DefineEnpoints(WebApplication app)
    {
        var name = Assembly.GetExecutingAssembly().GetName().Name + " v1";

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.OAuthClientId("swagger");
            c.OAuthClientSecret("secret");
            c.OAuthUsePkce();
            c.SwaggerEndpoint("/swagger/v1/swagger.json", name);
        });
    }

    public void DefineServices(IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwagger();
    }
}
