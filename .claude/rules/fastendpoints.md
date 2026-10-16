---
paths: ["**/*.cs"]
---

# FastEndpoints

- Recipes for common FastEndpoints patterns: https://fast-endpoints.com/docs/the-cookbook
- When you write or change a FastEndpoints endpoint and the repo has no existing pattern for it,
  check the cookbook before you invent one. A repo's existing pattern wins over the cookbook.

## What this repo uses (checked 2026-10-16)

- Validation is a FastEndpoints `Validator<TRequest>` (FluentValidation) in the endpoint's file,
  so the cookbook's validation recipes apply.
- Endpoint groups (`Group<T>()`): `MerchantCentre`, `PlatformOperations`, `Storefront`.
  `MerchantCentre` adds a group-wide pre-processor (`RequireMerchant`, `Order.Before`). Put
  cross-cutting checks for a route group there, not in each endpoint.
- Replies go through `Send.*Async`, and errors through ProblemDetails (`c.Errors.UseProblemDetails()`).
- OpenAPI comes from `FastEndpoints.OpenApi` (`OpenApiDocument(...)` + `MapOpenApi()`), served with
  Scalar. It isn't NSwag/`FastEndpoints.Swagger`, so the cookbook's Swagger recipes
  (`IOperationProcessor`, Swagger JSON export) don't apply as written.
- JWT uses ASP.NET Core `AddJwtBearer` to validate Keycloak tokens, not `FastEndpoints.Security`
  token issuing.
- Tests use `FastEndpoints.Testing` in-process (`ApiFixture : AppFixture<Program>`, `TestBase<ApiFixture>`,
  typed `GETAsync`/`POSTAsync`/`PUTAsync`/`DELETEAsync<TEndpoint, ...>` helpers), not `Factory.Create<TEndpoint>`
  unit tests.
- Not used yet: the command/event bus, post-processors, response interceptors, API versioning,
  job queues.
