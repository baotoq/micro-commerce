﻿namespace MicroCommerce.Basket.API.EndpointDefinitions;

public interface IEndpointDefinition
{
    void DefineEnpoints(WebApplication app);
    void DefineServices(IServiceCollection services);
}
