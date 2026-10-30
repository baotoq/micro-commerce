<!--
Sync Impact Report
Version change: template (unversioned) → 1.0.0
Modified principles: none renamed (initial adoption; all placeholders replaced)
Added sections: Core Principles I-V, Technology & Quality Constraints, Development Workflow, Governance
Removed sections: none
Follow-up TODOs: none
-->
# MicroCommerce Constitution

## Core Principles

### I. Modular Monolith with Module-Owned Data
Every business area (Accounts, Catalog, Ordering) MUST be a module under
`Features/<Area>/` with its own `DbContext`, database schema and migration history. A module
MUST NOT read or write another module's tables. Cross-module IDs MUST be typed IDs from
`SharedKernel`. Rationale: ADR-0003 and ADR-0005 keep modules extractable and boundaries clear.

### II. Events Between Modules, One Narrow Exception
Modules MUST communicate through Wolverine events published via the transactional outbox
(`IDbContextOutbox<TContext>`), never direct calls. Handlers MUST be idempotent because delivery
is at least once and unordered. The sole exception is Ordering reading Variants through the
Catalog's in-process `ICatalogLookup` contract; new exceptions require an ADR. Long-running
lifecycles (e.g. Checkout) MUST use Temporal workflows. Rationale: ADR-0004, ADR-0006.

### III. Vertical Slices on FastEndpoints
Each endpoint MUST live in one file holding its request record, `Validator<T>` and `Endpoint`,
and MUST follow `.claude/rules/fastendpoints.md`. Errors MUST be returned as ProblemDetails.
Entity-to-response mapping MUST use the module's Mapperly mapper, not hand-written mappers.
Value objects MUST be Vogen types and stay primitives in requests, validated by FluentValidation,
so invalid input is a 400.

### IV. Secure by Default Identity
Authentication MUST be Keycloak-issued JWT bearer tokens (ADR-0002). Endpoints MUST require
authorization unless they explicitly call `AllowAnonymous()`. A caller's Account, Merchant or
Cart MUST be derived from the token, never from route or body. Platform Operator actions MUST use
`AuthPolicies.PlatformOperator`.

### V. Test-Backed Changes (NON-NEGOTIABLE)
Behavior changes MUST be covered at the lowest suitable layer: Unit, Functional
(Testcontainers), Integration (AppHost) or Playwright E2E. Assertions MUST use Shouldly.
Asynchronous cross-module effects MUST be awaited by polling with a timeout, never fixed sleeps.
Concurrency-sensitive code (stock reservation, cart edits) MUST have tests that force real
overlap. Rationale: eventual consistency and concurrency are the main correctness risks.

## Technology & Quality Constraints

- Stack: .NET 10, Aspire, FastEndpoints, EF Core on PostgreSQL, Wolverine, Redis, Temporal and a
  Next.js frontend (pnpm). Versions are pinned in `mise.toml` and MUST stay in sync with
  `global.json` and the frontend `packageManager`.
- `TreatWarningsAsErrors` MUST remain enabled; package versions MUST live only in
  `Directory.Packages.props`.
- Money MUST be whole VND and never negative; stock checks MUST avoid overselling.
- Frontend code MUST consult the bundled Next.js docs before use, per its `AGENTS.md`.
- Schema changes MUST ship with an EF Core migration in the owning module.

## Development Workflow

- CI (`.github/workflows/ci.yml`) MUST pass: build, `dotnet format --verify-no-changes`,
  frontend lint/build, unit, functional, integration and E2E tests.
- Pull requests MUST fill `.github/pull_request_template.md`; UI changes MUST include
  Playwright screenshots of new or changed pages and states.
- Significant architectural decisions MUST be recorded as ADRs in `docs/adr/`; domain terms
  live in `CONTEXT.md`.

## Governance

This constitution supersedes other practices. Amendments MUST be made by pull request that
states the rationale, updates affected docs and, for removals or redefinitions of principles,
includes a migration plan. Versioning follows semantic versioning: MAJOR for incompatible
principle removals or redefinitions, MINOR for new or materially expanded principles or
sections, PATCH for clarifications. Reviewers MUST verify compliance in every PR, and any
violation MUST be justified in the PR. Runtime guidance lives in `CLAUDE.md`.

**Version**: 1.0.0 | **Ratified**: 2026-10-30 | **Last Amended**: 2026-10-30
