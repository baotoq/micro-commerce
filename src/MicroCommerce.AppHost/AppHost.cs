using CommunityToolkit.Aspire.Hosting.Dapr;
using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

// Integration tests pass --EphemeralResources=true to get throwaway containers
// instead of reusing the persistent dev containers and their data volumes.
var ephemeral = builder.Configuration.GetValue<bool>("EphemeralResources");

// Ephemeral runs get random ports unless they also pass --FixedPorts=true:
// the browser sign-in needs Keycloak on 8080 and the web app on 5173 (see the realm file).
var fixedPorts = builder.Configuration.GetValue("FixedPorts", !ephemeral);

// --WebPort pins only the web app's port. The browser e2e suite passes 5174, the realm's second
// redirect URI, so it can run next to a dev Marketplace on 5173 and 8080; Keycloak keeps a random port.
var webPort = builder.Configuration.GetValue<int?>("WebPort");

// Generated once and kept in user secrets; shown in the dashboard's Parameters view. Never committed.
var buyerPassword = AddSecret("dev-buyer-password");
var operatorPassword = AddSecret("dev-operator-password");
var merchantPassword = AddSecret("dev-merchant-password");
var daprAppApiToken = AddSecret("dapr-app-api-token");

var postgres = builder.AddPostgres("postgres");
// Plain TCP: the Dapr pub/sub component below doesn't enable TLS, and Redis is only reached locally.
#pragma warning disable ASPIRECERTIFICATES001 // WithoutHttpsCertificate is experimental
var redis = builder.AddRedis("redis").WithoutHttpsCertificate();
#pragma warning restore ASPIRECERTIFICATES001
var temporal = builder.AddContainer("temporal", "temporalio/temporal", "1.5.1")
    .WithArgs("server", "start-dev", "--ip", "0.0.0.0")
    .WithEndpoint(targetPort: 7233, name: "grpc")
    .WithHttpEndpoint(targetPort: 8233, name: "ui");

// The dev realm (clients, Platform Operator role, seeded Accounts) is imported when Keycloak doesn't
// have it yet. Its passwords are ${MC_*} placeholders that Keycloak fills from the environment.
// The persistent volume keeps an imported realm, so delete the volume after editing the realm file.
// The stable port keeps the token issuer, and so the browser's OIDC session, the same across runs.
var keycloak = builder.AddKeycloak("keycloak", port: fixedPorts ? 8080 : null)
    .WithRealmImport("./Realms")
    .WithEnvironment("MC_BUYER_PASSWORD", buyerPassword)
    .WithEnvironment("MC_OPERATOR_PASSWORD", operatorPassword)
    .WithEnvironment("MC_MERCHANT_PASSWORD", merchantPassword);
var keycloakIssuer = ReferenceExpression.Create($"{keycloak.GetEndpoint("http")}/realms/micro-commerce");

if (!ephemeral)
{
    postgres.WithDataVolume().WithLifetime(ContainerLifetime.Persistent);
    redis.WithDataVolume().WithLifetime(ContainerLifetime.Persistent);
    keycloak.WithDataVolume().WithLifetime(ContainerLifetime.Persistent);
    temporal.WithArgs("--db-filename", "/temporal-data/temporal.db")
        .WithVolume("temporal-data", "/temporal-data").WithLifetime(ContainerLifetime.Persistent);
}

var db = postgres.AddDatabase("micro-commerce-db");

// Product images (constitution 1.1.0): Azure Blob Storage, run locally as the Azurite emulator.
// Persistent like the other resources, unless this is an ephemeral (test) run.
var storage = builder.AddAzureStorage("storage").RunAsEmulator(emulator =>
{
    // Aspire 13.6.0 defaults to Azurite 3.35.0, which rejects the Blob SDK's API version (2026-06-06).
    emulator.WithImageTag("3.37.0");
    if (!ephemeral)
    {
        emulator.WithDataVolume().WithLifetime(ContainerLifetime.Persistent);
    }
});
var blobs = storage.AddBlobs("blobs");

// Registers the lifecycle hook that starts each sidecar with the Dapr CLI (`dapr init --slim`).
builder.AddDapr();

// Dapr pub/sub on Aspire's own Redis, which requires a password (research R6, spike T011(b)).
// AddDaprPubSub would generate an in-memory component and drop the metadata, so the component
// type is named explicitly. Both values reach daprd through the toolkit's environment secret store,
// never through a file.
var redisEndpoint = redis.GetEndpoint("tcp");
var pubsub = builder.AddDaprComponent("pubsub", "pubsub.redis")
    .WithMetadata("redisHost", ReferenceExpression.Create($"{redisEndpoint.Property(EndpointProperty.Host)}:{redisEndpoint.Property(EndpointProperty.Port)}"))
    .WithMetadata("redisPassword", redis.Resource.PasswordParameter!);

var api = builder.AddProject<Projects.MicroCommerce_ApiService>("apiservice")
    .WithDaprSidecar(sidecar => sidecar
        .WithOptions(new DaprSidecarOptions { AppId = "apiservice" })
        // The sidecar resource has no WithEnvironment; the toolkit copies environment callbacks
        // from it onto the daprd process. daprd then sends the token as the dapr-api-token header.
        .WithAnnotation(new EnvironmentCallbackAnnotation(c => c.EnvironmentVariables["APP_API_TOKEN"] = daprAppApiToken.Resource))
        .WithReference(pubsub))
    .WithEnvironment("APP_API_TOKEN", daprAppApiToken)
    .WithExternalHttpEndpoints()
    // Over plain HTTP, so the check doesn't depend on the machine trusting the ASP.NET dev certificate.
    .WithHttpHealthCheck("/health", endpointName: "http")
    .WithReference(db)
    .WaitFor(db)
    .WithReference(blobs)
    .WaitFor(blobs)
    .WaitFor(redis)
    .WithEnvironment("Temporal__Address", temporal.GetEndpoint("grpc").Property(EndpointProperty.HostAndPort))
    .WithEnvironment("Temporal__Namespace", "default")
    .WaitFor(temporal)
    .WithEnvironment("Authentication__Schemes__Bearer__Authority", keycloakIssuer)
    .WaitFor(keycloak);

var web = builder.AddViteApp("web", "../MicroCommerce.Web")
    .WithPnpm()
    .WithReference(api)
    .WithEnvironment("VITE_OIDC_AUTHORITY", keycloakIssuer)
    .WithEnvironment("VITE_OIDC_CLIENT_ID", "micro-commerce-web")
    .WithExternalHttpEndpoints()
    // Healthy only once Vite serves the page, not as soon as its process starts.
    .WithHttpHealthCheck("/")
    .WaitFor(api)
    .WaitFor(keycloak);

// The realm registers http://localhost:5173 (development) and http://localhost:5174 (e2e suite)
// as the web app's redirect URIs.
if (webPort is { } port)
{
    // Not proxied: Aspire's testing builder randomizes the host port of proxied endpoints, and the
    // realm only accepts this exact port. Vite listens on it directly (PORT, strictPort).
    web.WithEndpoint("http", e =>
    {
        e.Port = port;
        e.TargetPort = port;
        e.IsProxied = false;
    });
}
else if (fixedPorts)
{
    web.WithEndpoint("http", e => e.Port = 5173);
}

builder.Build().Run();

IResourceBuilder<ParameterResource> AddSecret(string name) =>
    builder.AddParameter(name, new GenerateParameterDefault { MinLength = 24, Special = false }, secret: true, persist: true);
