using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

// Integration tests pass --EphemeralResources=true to get throwaway containers
// instead of reusing the persistent dev containers and their data volumes.
var ephemeral = builder.Configuration.GetValue<bool>("EphemeralResources");

var postgres = builder.AddPostgres("postgres");
var redis = builder.AddRedis("redis");

// The dev realm (clients, Platform Operator role, seeded users) is imported when Keycloak doesn't have it yet.
// The persistent volume keeps an imported realm, so delete the volume after editing the realm file.
// The stable port keeps the token issuer, and so the browser's OIDC cookies, the same across runs.
var keycloak = builder.AddKeycloak("keycloak", port: ephemeral ? null : 8080)
    .WithRealmImport("./Realms");
var keycloakIssuer = ReferenceExpression.Create($"{keycloak.GetEndpoint("http")}/realms/micro-commerce");

if (!ephemeral)
{
    postgres.WithDataVolume().WithPgAdmin().WithLifetime(ContainerLifetime.Persistent);
    redis.WithDataVolume().WithRedisInsight().WithLifetime(ContainerLifetime.Persistent);
    keycloak.WithDataVolume().WithLifetime(ContainerLifetime.Persistent);
}

var db = postgres.AddDatabase("micro-commerce-db");

var api = builder.AddProject<Projects.MicroCommerce_ApiService>("apiservice")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(db)
    .WaitFor(db)
    .WithReference(redis)
    .WaitFor(redis)
    .WithEnvironment("Authentication__Schemes__Bearer__Authority", keycloakIssuer)
    .WaitFor(keycloak);

// Signs and encrypts the frontend's session cookie; generated once and kept in user secrets.
var authSecret = builder.AddParameter(
    "frontend-auth-secret", new GenerateParameterDefault { MinLength = 32, Special = false }, secret: true, persist: true);

#pragma warning disable ASPIREJAVASCRIPT001 // AddNextJsApp is experimental
var frontend = builder.AddNextJsApp("frontend", "../micro-commerce", runScriptName: "dev")
    .WithPnpm()
    .WithEnvironment("API_URL", api.GetEndpoint("http"))
    .WithEnvironment("AUTH_SECRET", authSecret)
    .WithEnvironment("AUTH_KEYCLOAK_ID", "micro-commerce-frontend")
    .WithEnvironment("AUTH_KEYCLOAK_SECRET", "micro-commerce-frontend-dev-secret") // dev-only, matches the realm file
    .WithEnvironment("AUTH_KEYCLOAK_ISSUER", keycloakIssuer)
    .WithExternalHttpEndpoints()
    .WaitFor(api)
    .WaitFor(keycloak);
#pragma warning restore ASPIREJAVASCRIPT001

// Auth.js builds its OIDC redirect URI from this; the Host it sees is the dev server's internal port.
frontend.WithEnvironment("AUTH_URL", frontend.GetEndpoint("http"));

// The realm registers http://localhost:3000 as the frontend's only redirect URI.
if (!ephemeral)
{
    frontend.WithEndpoint("http", e => e.Port = 3000);
}

builder.Build().Run();
