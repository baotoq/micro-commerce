# MicroCommerce Constitution

## Core Principles

### I. Ubiquitous Language

`CONTEXT.md` is the glossary of the Marketplace and the source of truth for domain terms.

- Code, specs, API routes, UI text, tests and commit messages MUST use the glossary's terms, such as
  Account, Merchant, Variant, Checkout and Order. They MUST NOT use the terms the glossary lists under
  _Avoid_, such as User, Seller, SKU or Sub-order.
- A new domain concept, or a change of meaning to an existing one, MUST update `CONTEXT.md` in the same
  change that introduces it.
- Where the glossary and code disagree, the glossary wins and the code is the defect.

**Rationale**: A marketplace has many near-synonyms (order, checkout, payment, refund). One shared
vocabulary keeps specs, code and conversations about the same thing.

### II. Domain-Centred Design

- Business rules MUST live in the domain layer, inside aggregates and value objects. They MUST NOT
  live in endpoints, handlers, mappers or database configuration.
- The domain layer MUST NOT depend on persistence, transport, messaging, workflow or web frameworks.
- An aggregate is changed only through its own methods, which enforce its invariants. One transaction
  changes one aggregate. Other aggregates react through domain or integration events.
- Every entity identifier MUST be its own strongly typed value, so that one entity's ID cannot be
  passed where another's is expected. Raw `Guid`, `string` or `int` IDs are allowed only at the
  API and database edges, with explicit conversion there.
- Each bounded area (for example Catalog, Buying, Shipping, Money) owns its data. Another area MUST NOT
  read or write it directly. It goes through a published contract or event.

**Rationale**: The Marketplace rules, such as Escrow, Reservations, deadlines and all-or-nothing
Checkout, are the product. Keeping them isolated and explicit makes them testable and safe to change.

### III. Test-First, Fast and Isolated Tests (NON-NEGOTIABLE)

- Every behaviour change MUST come with automated tests that fail without it. Tests for a user
  story's acceptance scenarios are written before or together with its implementation, never after
  the merge.
- Unit tests MUST run with no database, network, container or clock dependency. Collaborators are
  replaced with test doubles, and time is injected.
- Integration tests MUST provision their own isolated, disposable infrastructure and clean it up.
  They MUST NOT depend on a developer's running environment or on each other's order.
- Every backend operation MUST have at least one integration test covering its success path and its
  authorisation refusal.
- A flaky test is a defect. It is fixed or removed, never retried until it passes.

**Rationale**: Money and stock rules break silently. Tests that are fast and deterministic get run,
and tests that get run catch regressions.

### IV. Secure by Default

- Every backend operation MUST require an authenticated Account unless it is explicitly marked
  public, such as Storefront browsing. Authorisation for roles (Buyer, Merchant owner, Platform
  Operator) is checked on the server for every request.
- Request-scoped context, such as the signed-in Account, its Merchant and correlation data, MUST come
  from the validated credential and request context. It MUST NOT come from fields in the request
  body. Commands and queries carry only client-provided input.
- All input and external data MUST be validated at the trust boundary before it reaches the domain.
- Secrets, tokens and connection strings MUST NOT appear in source, configuration files, logs, tests
  or commit messages. Local values come from the local orchestration or user secrets.
- Passwords and sign-in flows are delegated to the identity provider. The services never store
  credentials.

**Rationale**: A marketplace moves other people's money. One endpoint that trusts a body-supplied
`merchantId` is enough to let a Merchant act on another Merchant's Orders.

### V. Durable and Idempotent Processes

- Time-based lifecycle rules MUST run as durable workflows that survive process restarts. This covers
  the payment window, ship-by deadline, confirmation period, merchant response deadline and similar
  rules. They MUST NOT run as in-memory timers or polling loops.
- Workflow and event-handler side effects MUST be idempotent. Delivering the same message or
  repeating the same activity more than once has the same effect as doing it once.
- State changes that must be announced to other areas are published as events and assume
  at-least-once delivery. A handler MUST tolerate duplicates and out-of-order arrival.
- Money movements (Escrow, Commission, Refund, Payout) MUST be traceable to the event or workflow
  step that caused them.

**Rationale**: Deadlines and payments outlive any single process. Lost or doubled timers become lost
stock, double refunds or Orders that never complete.

### VI. Observable Behaviour

- Application code logs only through `ILogger<T>`, with structured message templates and never string
  interpolation.
- Logs, traces and metrics MUST NOT contain secrets, tokens or personal data, such as addresses,
  emails or payment details.
- Every service MUST emit traces and metrics, report its health, and appear in the local orchestration
  dashboard.
- Errors returned to clients use one consistent problem-details format and do not leak stack traces
  or internals.

**Rationale**: Distributed flows across Checkout, Payment, Orders and workflows can only be debugged
from correlated, structured telemetry.

### VII. Simplicity and Deliberate Dependencies

- Build what the current spec needs. Speculative abstractions, layers and configuration options
  MUST NOT be added. Duplication is preferred over the wrong abstraction.
- A new third-party dependency MUST be approved before it is added. All package versions are pinned
  centrally in `Directory.Packages.props`.
- Before writing new infrastructure or conventions, an existing pattern in the repository MUST be
  reused.
- Each deviation from these principles MUST be justified in the plan's Complexity Tracking table.

**Rationale**: Every dependency and abstraction is maintained forever. A small, consistent codebase
is cheaper to change than a clever one.

## Technology Constraints

The approved stack. Changing an item here is a constitution amendment.

- **Runtime and language**: .NET 10 and C#, with nullable reference types enabled and warnings
  treated as errors.
- **Local orchestration**: .NET Aspire. One AppHost command starts every component.
- **Backend operations**: FastEndpoints, with validators in the endpoint's file and ProblemDetails
  errors. OpenAPI is browsable through Scalar.
- **Persistence**: EF Core on PostgreSQL. Schema changes go through migrations.
- **Typed identifiers**: Vogen.
- **Mapping**: Mapster.
- **Durable workflows**: Temporal.
- **Asynchronous messaging**: Dapr (pub/sub).
- **Caching**: Redis, only where a spec justifies it.
- **Identity**: Keycloak (OpenID Connect). Backends validate JWT bearer tokens.
- **Web app**: React with Vite.
- **Testing**: xUnit v3, NSubstitute for test doubles, Shouldly for assertions and Testcontainers for
  integration infrastructure.

## Development Workflow

- Work is specified before it is built, with Spec Kit: `/speckit-specify`, then `/speckit-plan`,
  then `/speckit-tasks`, then `/speckit-implement`. Each plan MUST pass the Constitution Check before
  design starts and again after design.
- Each ticket or feature goes on its own short-lived branch off `master` and lands through a pull
  request that is squash-merged. `master` MUST always build and pass all tests.
- A pull request MUST include its tests, updates to `CONTEXT.md` for any new domain terms, and
  migrations for any schema change.
- Reviewers MUST check each pull request against this constitution. A violation is either fixed or
  recorded with justification in the plan's Complexity Tracking table.

## Governance

- This constitution overrides other project practices and guidance. Where a feature spec or plan
  conflicts with it, the constitution wins until it is amended.
- **Amendments** are made in a pull request that edits this file, explains the reason and updates
  any affected templates or guidance in the same change.
- **Versioning** follows semantic versioning:
  - MAJOR for removing or redefining a principle in a way that is not backward compatible.
  - MINOR for adding a principle or section, or materially expanding guidance.
  - PATCH for clarifications and wording.
- **Compliance**: `/speckit-plan` runs the Constitution Check gate. `/speckit-analyze` flags
  violations in spec, plan and tasks. Code review confirms them before merge.

**Version**: 1.0.0 | **Ratified**: 2026-11-04 | **Last Amended**: 2026-11-04
