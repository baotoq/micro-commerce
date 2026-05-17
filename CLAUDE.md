# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repo layout

All source code lives under `src/`:

- `src/web/` — Next.js 16 frontend
- `src/AppHost/` — .NET Aspire AppHost (orchestrates the full stack)
- `src/Services/Catalog.API/` — .NET Catalog microservice (Clean Architecture)
- `src/MicroCommerce.slnx` — .NET solution file

Design references, QA assets, and documentation live at the repo root (`design/`, `qa/`, `DESIGN.md`).

## Commands

### Full stack (requires Docker)

```bash
dotnet run --project src/AppHost
```

This starts Redis, PostgreSQL, the Catalog API (with Dapr sidecar), and the Next.js app via Aspire.

### Frontend (run from `src/web/`)

- `npm run dev` — dev server (http://localhost:3000)
- `npm run build` — production build (also the typecheck gate)
- `npm run lint` — Biome check (lint + import organization)
- `npm run format` — Biome format with `--write`
- `npm test` — Vitest unit + component tests (jsdom)
- `npm run e2e` — Playwright e2e specs in `e2e/` (Chromium-only). Requires a running stack; the listings specs hit the real Catalog API. Either start the Aspire AppHost (`dotnet run --project src/AppHost`) and trigger the `playwright` resource from the dashboard, or run AppHost in one terminal and `BASE_URL=<web-endpoint> npm run e2e` in another.

### .NET backend

```bash
# Build all
dotnet build src/MicroCommerce.slnx

# Run all tests
dotnet test src/MicroCommerce.slnx

# Run a specific test project
dotnet test src/Services/Catalog.API/tests/Domain.Tests
dotnet test src/Services/Catalog.API/tests/Application.Tests
dotnet test src/Services/Catalog.API/tests/FunctionalTests    # needs Docker (Testcontainers)
dotnet test src/Services/Catalog.API/tests/IntegrationTests   # needs Docker (full Aspire stack)

# Run a single test by name
dotnet test src/Services/Catalog.API/tests/Domain.Tests --filter "FullyQualifiedName~TestMethodName"
```

## Architecture

### Distributed system

The Aspire AppHost wires together:
- **Redis** — output caching for the API
- **PostgreSQL** (`catalogdb`) — Catalog API persistence
- **catalog-api** — .NET service, references Redis + Postgres, has a Dapr sidecar, health check at `/health`
- **web** — Next.js app, receives `API_URL` env var pointing to catalog-api; skipped when `DOTNET_ENVIRONMENT=Testing`

### Catalog.API layers (Clean Architecture, `net10.0`)

```
Domain      → value objects (Vogen v8), domain entities, no dependencies
Application → MediatR v12 commands/queries (CQRS), depends on Domain
Infrastructure → EF Core + Npgsql, AppDbContext, depends on Domain
Api         → ASP.NET Minimal API endpoints, Aspire service defaults, depends on Application + Infrastructure
```

Key decisions that affect how you write code:
- **No repository pattern** — Application handlers take `AppDbContext` directly via constructor injection.
- **Vogen value objects** — `ProductId` and `Sku` are Vogen-generated structs. Construct them with `Sku.From(string)`, not `new Sku(...)`.
- **MediatR v12** — commands and queries are records implementing `IRequest<T>`; handlers implement `IRequestHandler<TRequest, TResponse>`.
- **Minimal API endpoints** — grouped under `/api/products`, registered via extension methods (`MapProductReadEndpoints`, `MapProductWriteEndpoints`).

### Catalog.API test pyramid

| Project | Scope | Infrastructure |
|---|---|---|
| `Domain.Tests` | Pure unit | None |
| `Application.Tests` | Handler logic | In-memory EF Core |
| `FunctionalTests` | HTTP end-to-end | `WebApplicationFactory` + Testcontainers PostgreSQL |
| `IntegrationTests` | Full stack | `DistributedApplicationTestingBuilder` (Aspire) — requires Docker + Dapr CLI |

## Frontend stack notes

- **Next.js 16 (App Router)** with React 19. This version has breaking changes vs. older training data; consult `src/web/node_modules/next/dist/docs/` before writing non-trivial Next.js code.
- **React Compiler is enabled** (`reactCompiler: true` in `src/web/next.config.ts`). Don't hand-roll `useMemo`/`useCallback`/`React.memo` for render perf — the compiler handles memoization. Reserve those hooks for cases where referential identity is semantically required.
- **Tailwind CSS v4** via `@tailwindcss/postcss`. Configuration is CSS-first in `src/web/src/app/globals.css`; there is no `tailwind.config.{js,ts}`. Theme tokens belong in `globals.css` using v4's `@theme` / `@import` syntax.
- **Biome (not ESLint/Prettier)** is the only linter/formatter. 2-space indent, imports auto-organized. Run `npm run lint` before declaring work done.
- **TypeScript path alias**: `@/*` → `src/web/src/*`.
- App Router source lives under `src/web/src/app/`. Use Server Components by default; add `"use client"` only when a component needs hooks, browser APIs, or event handlers.
- Use `next/image` and `next/font` (Geist Sans/Mono already wired in `src/web/src/app/layout.tsx`).

## Testing
MUST use TDD to write tests before implementing features.
DO NOT add tax maintenance tests.

## UI Design
`DESIGN.md` contains the UI design specifications. Read it before implementing any UI feature.
`design/` contains hi-fi designs as JSX components in `design/project/hifi-*.jsx`, aggregated by `design/project/Micro Commerce Hi-fi.html`.

## Manual QA

Manual QA assets live at `qa/` (repo root). Structure:

- `qa/README.md` — handbook (test-case template, status semantics, agent-browser patterns)
- `qa/test-cases/TC-S<NN>-<route-slug>.md` — one file per route
- `qa/test-runs/<YYYY-MM-DD>-execution-report.md` — aggregate run report
- `qa/evidence/screenshots/S<NN>-<MM>.png` and `qa/evidence/console-logs/S<NN>.txt`

To run a fresh manual-QA pass:

1. **Start dev server**: `cd src/web && npm run dev`
2. **Use agent-browser** (absolute path `/usr/local/bin/agent-browser`):

   ```bash
   AB=/usr/local/bin/agent-browser
   $AB --session qa open http://localhost:3000/seller/<route>
   $AB --session qa wait --load networkidle
   $AB --session qa errors
   $AB --session qa snapshot
   $AB --session qa screenshot --annotate qa/evidence/screenshots/<case-id>.png
   ```

3. **Or use Playwright MCP** for screenshot-only audits. Save under `.playwright-mcp/` or repo root.
4. Update the test-case markdown (Actual + Status + evidence path) and append a `## Summary` block.
5. Aggregate in `qa/test-runs/<date>-execution-report.md`.

Conventions:

- Tests are read-only against the running app.
- Automated e2e specs live in `src/web/e2e/seller-*.spec.ts`. Manual QA covers visual/a11y/console-error angles those miss.
- Hi-fi reference per route: `SFlow_NN`/`Listings_*`/`Analytics_*` components in the `design/project/hifi-*.jsx` files. Verbatim string copy matters — middle-dot ·, en-dash –, ★, ↑/↓ glyphs are intentional.
- The `/seller/orders/[id]/pack` page renders the "Buy your shipping label" modal always-open as a static visual.
- Brand: `BRAND = { name: "Micro Commerce", owner: "Alex" }`. "Mira" appears intentionally only on `/seller/apply`; anywhere else it is a copy bug.
