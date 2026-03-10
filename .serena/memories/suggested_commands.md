# MicroCommerce — Suggested Commands

## Run
```bash
# Full stack (primary entry point)
dotnet run --project src/MicroCommerce.AppHost

# Backend only
dotnet run --project src/MicroCommerce.ApiService

# Frontend only
cd src/MicroCommerce.Web && npm install && npm run dev
```

## Test
```bash
# Unit + integration tests (backend)
dotnet test src/MicroCommerce.ApiService.Tests

# E2E tests (requires full Aspire stack running)
cd src/MicroCommerce.Web && npx playwright test
```

## Lint & Format (Frontend)
```bash
cd src/MicroCommerce.Web && npm run lint
cd src/MicroCommerce.Web && npm run format
```

## EF Core Migrations
Migrations are per-feature under `Features/{Name}/Infrastructure/Migrations/`.
Use the feature-specific DbContext when adding migrations.

## Notes
- OpenAPI docs: `/openapi/v1.json` (dev only)
- Health: `/health` (readiness), `/alive` (liveness)
- Keycloak realm config: `src/MicroCommerce.AppHost/Realms/`
- Frontend `.env`: AUTH_SECRET, KEYCLOAK_CLIENT_SECRET, KEYCLOAK_ISSUER
