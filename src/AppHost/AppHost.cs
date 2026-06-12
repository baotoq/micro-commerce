#pragma warning disable ASPIREJAVASCRIPT001

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache").WithDataVolume().WithLifetime(ContainerLifetime.Persistent);

var pgPassword = builder.AddParameter("postgres-password", secret: true);
var postgres = builder.AddPostgres("postgres", password: pgPassword)
    .WithDataVolume().WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("catalogdb");

var storage = builder.AddAzureStorage("storage").RunAsEmulator(emu =>
    emu.WithDataVolume().WithLifetime(ContainerLifetime.Persistent));
var photos = storage.AddBlobs("photos");

var pubSub = builder.AddDaprPubSub("pubsub");

// Keycloak IdP for seller auth, hosted as a plain Aspire container in dev mode over
// HTTP on a STABLE host port 8080 → a single issuer URL (http://localhost:8080/realms/
// microcommerce) the browser, Next server, and API all agree on, with no self-signed
// TLS to work around. start-dev + --import-realm seeds the realm from the bind-mounted
// JSON on first run (existing realms are skipped). Declared OUTSIDE the Testing guard so
// IntegrationTests can obtain real tokens via the seeded dev seller.
var keycloak = builder.AddContainer("keycloak", "quay.io/keycloak/keycloak", "26.3.3")
    .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "http")
    .WithBindMount("Realms", "/opt/keycloak/data/import")
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_USERNAME", "admin")
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_PASSWORD", "admin")
    .WithArgs("start-dev", "--import-realm");

var catalog = builder.AddProject<Projects.MicroCommerce_Catalog>("catalog-api")
    .WithReference(cache)
    .WithReference(postgres)
    .WithReference(photos)
    // Keycloak is a plain container (no client integration), so the API reads its
    // authority from explicit config rather than Aspire service discovery.
    .WithEnvironment("Keycloak__Authority",
        ReferenceExpression.Create($"{keycloak.GetEndpoint("http")}/realms/microcommerce"))
    .WaitFor(cache)
    .WaitFor(postgres)
    .WaitFor(photos)
    .WaitFor(keycloak)
    .WithHttpHealthCheck("/health")
    .WithExternalHttpEndpoints()
    .WithDaprSidecar(sidecar => sidecar.WithReference(pubSub));

if (builder.Environment.EnvironmentName != "Testing")
{
    catalog.WithEnvironment("SEED_PRODUCTS", "true");

    const string realm = "microcommerce";
    var web = builder.AddNextJsApp("web", "../web")
        .WithHttpEndpoint(port: 3000, env: "PORT")
        .WithEnvironment("API_URL", catalog.GetEndpoint("http"))
        // Auth.js v5 infers the Keycloak provider from AUTH_KEYCLOAK_*.
        .WithEnvironment("AUTH_KEYCLOAK_ISSUER",
            ReferenceExpression.Create($"{keycloak.GetEndpoint("http")}/realms/{realm}"))
        .WithEnvironment("AUTH_KEYCLOAK_ID", "microcommerce-web")
        // dev-only secrets — must match the imported realm; never use in production.
        .WithEnvironment("AUTH_KEYCLOAK_SECRET", "dev-only-web-secret")
        .WithEnvironment("AUTH_SECRET", "dev-only-auth-secret-not-for-prod-0123456789abcdef")
        // Pin the canonical origin so OAuth callbacks land on the registered redirect
        // URI even when the dashboard opens a *.dev.localhost host.
        .WithEnvironment("AUTH_URL", "http://localhost:3000")
        .WithExternalHttpEndpoints()
        .WaitFor(keycloak);

    builder.AddJavaScriptApp("playwright", "../web", "e2e")
        .WithReference(web)
        .WithReference(catalog)
        .WithEnvironment("E2E_SELLER_EMAIL", "seller@microcommerce.dev")
        .WithEnvironment("E2E_SELLER_PASSWORD", "Passw0rd!")
        // The Aspire-driven suite shares one Catalog DB across all workers,
        // so parallel mutation specs inflate counts read by the count-tied
        // listings/pagination specs. Serial execution costs ~30s but kills
        // the flake. (Local `npm run e2e` runs still use Playwright's
        // default worker count.)
        .WithEnvironment("CI", "1")
        .WithEnvironment("PLAYWRIGHT_WORKERS", "1")
        .WaitFor(web)
        .WithExplicitStart()
        .ExcludeFromManifest();
}

builder.Build().Run();
