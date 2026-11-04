# Feature Specification: Project Skeleton (Walking Skeleton)

**Feature Branch**: `001-project-skeleton`

**Created**: 2026-11-04

**Status**: Draft

**Input**: User description: "init project with .net 10, aspire, unit tests with xunit v3 nsubtitube temporal mapster DDD, vogen react vite keyload ef core fastendpoint"

## Overview

MicroCommerce is a multi-merchant Marketplace (see `CONTEXT.md` for the domain language). Before any
Marketplace capability can be built, the team needs a working, end-to-end skeleton. It has to run
locally with one command, let an Account sign in, persist data, run durable background processes and
come with automated tests. This feature delivers that skeleton and the conventions every later
feature will follow. It delivers no Marketplace capability (Catalog, Cart, Checkout and so on) beyond
what is needed to prove that the skeleton works.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Start the whole system with one command (Priority: P1)

A developer clones the repository and starts the entire system with a single command: the backend
service, the web app, the database, the identity provider and the durable-workflow engine. They see
every part's health and logs in one place.

**Why this priority**: Nothing else can be built or demonstrated until the system runs. This is the
foundation of every other story.

**Independent Test**: On a machine with only the documented prerequisites installed, run the single
start command. Then confirm that every component reports healthy and that the web app's home page
loads in a browser.

**Acceptance Scenarios**:

1. **Given** a fresh clone and the documented prerequisites, **When** the developer runs the start
   command, **Then** all components start and report healthy without any further manual setup.
2. **Given** the system is running, **When** the developer opens the orchestration overview,
   **Then** they see each component's status, logs and traces in one place.
3. **Given** the system is running, **When** the developer opens the web app, **Then** its home page
   loads and shows that it can reach the backend service.
4. **Given** a required prerequisite is missing (for example, the container runtime is not running),
   **When** the developer runs the start command, **Then** they get a clear message naming what is
   missing.

---

### User Story 2 - Sign in as an Account end to end (Priority: P1)

A person opens the web app, signs in through the Marketplace identity provider and sees their own
Account details, which the backend returns. Anonymous calls to protected backend operations are
rejected.

**Why this priority**: Almost every Marketplace capability (Cart, Checkout, Merchant Centre,
Platform Operations) depends on knowing who the Account is. Proving the full sign-in round trip early
removes the largest integration risk.

**Independent Test**: With the system running, sign in with a seeded development Account. Confirm
that the web app shows that Account's identity as returned by the backend. Then call a protected
backend operation without credentials and confirm it is refused.

**Acceptance Scenarios**:

1. **Given** a seeded development Account, **When** the person signs in from the web app, **Then**
   they are returned to the web app signed in and see their Account's display name and identifier as
   reported by the backend.
2. **Given** no sign-in, **When** a protected backend operation is called, **Then** it is refused as
   unauthenticated and no data is returned.
3. **Given** a signed-in Account, **When** the person signs out, **Then** protected pages and
   operations are no longer available to them until they sign in again.
4. **Given** a seeded Platform Operator Account, **When** it signs in, **Then** the backend
   recognises it as a Platform Operator, while an ordinary Account is not recognised as one.

---

### User Story 3 - Trustworthy automated tests from day one (Priority: P2)

A developer runs the whole automated test suite with one command. Fast unit tests need no external
services. Integration tests start their own disposable infrastructure and clean it up afterwards.

**Why this priority**: The skeleton sets the testing conventions every later feature copies. Getting
them right now prevents slow, flaky or infrastructure-dependent tests from spreading.

**Independent Test**: On a machine without the system running, run the test command. Unit tests pass
with no network or container access. Integration tests pass using their own disposable
infrastructure and leave nothing running afterwards.

**Acceptance Scenarios**:

1. **Given** no external services are running, **When** the developer runs only the unit tests,
   **Then** they pass without needing a database, identity provider, network or container runtime.
2. **Given** a container runtime is available, **When** the developer runs the integration tests,
   **Then** they create their own isolated infrastructure, pass, and remove that infrastructure when
   they finish.
3. **Given** a test fails, **When** the developer reads the output, **Then** it names the failing
   behaviour and the expected and actual results.

---

### User Story 4 - Durable background process survives restarts (Priority: P3)

A developer triggers a sample long-running, time-based process through the backend (for example
"wait N seconds, then record completion"). They restart the backend service while the process is
waiting and observe that the process still completes once, on time, after the restart.

**Why this priority**: Upcoming Marketplace rules depend on durable timers, such as the Payment
window, the Ship-by deadline, the Confirmation period and the Merchant response deadline. Proving
the durable-process plumbing early de-risks them, but nothing user-facing needs it on day one.

**Independent Test**: Start a sample process with a short delay, restart the backend service during
the delay, and confirm that the process records completion exactly once after the delay elapses.

**Acceptance Scenarios**:

1. **Given** the system is running, **When** a developer starts the sample process, **Then** they
   receive an identifier they can use to query its status.
2. **Given** a sample process is waiting, **When** the backend service is restarted, **Then** the
   process resumes and completes exactly once after its delay.
3. **Given** a sample process has completed, **When** its status is queried, **Then** it reports
   completed together with its completion time.

---

### Edge Cases

- The identity provider is still starting when the web app or backend comes up. Sign-in must wait
  or retry, not fail permanently.
- The database is unreachable at startup. The backend must report itself unhealthy and recover
  without a restart once the database is reachable.
- A token is expired or has been tampered with. Protected operations must refuse it the same way they
  refuse a missing token.
- The same Account signs in in two browser tabs. Both sessions show the same Account.
- The backend restarts while a durable process is waiting. The process must neither be lost nor run
  twice (User Story 4).
- The database schema is behind the code. Starting the system must bring a local development
  database up to date automatically. Other environments are out of scope for this feature.
- An event is delivered twice, or its handler fails once. It must be handled exactly once in effect
  (FR-012b).
- Two developers on the same machine, or tests run in parallel. Integration tests must not share or
  collide on infrastructure.

## Requirements *(mandatory)*

### Functional Requirements

**Local environment**

- **FR-001**: The system MUST start every component locally with one documented command: the backend
  service, the web app, the database, the identity provider, the durable-workflow engine and the
  asynchronous messaging runtime.
- **FR-002**: The system MUST provide one place where a developer sees every component's health, logs
  and traces while running locally.
- **FR-003**: Every backend component MUST expose health status that the local orchestration uses to
  report readiness.
- **FR-004**: Starting the system locally MUST create or upgrade the database schema and seed the
  identity provider with at least one ordinary development Account and one Platform Operator
  Account, with no manual steps.
- **FR-005**: The repository MUST document the prerequisites and the start and test commands in one
  README.

**Identity and access**

- **FR-006**: Accounts MUST sign in to the web app through the Marketplace identity provider. The
  backend and web app MUST NOT store or handle passwords themselves.
- **FR-007**: The backend MUST refuse every protected operation that is called without a valid,
  unexpired credential from the Marketplace identity provider.
- **FR-008**: The backend MUST expose an operation that returns the signed-in Account's identifier
  and display name, and whether it is a Platform Operator.
- **FR-009**: The backend MUST resolve the signed-in Account from the request's credential, never from
  identifiers supplied in the request body.
- **FR-010**: Accounts MUST be able to sign out of the web app.

**Durable processes**

- **FR-011**: The backend MUST be able to start a sample durable, time-based process and report its
  status (running or completed, with the completion time).
- **FR-012**: A started durable process MUST survive restarts of the backend service and complete
  exactly once.

**Asynchronous messaging**

- **FR-012a**: The backend MUST be able to publish an event and handle it asynchronously through the
  messaging runtime. The skeleton proves this with one sample event: the sample durable process
  publishes a "completed" event when it finishes, and a subscriber records that it received it.
- **FR-012b**: An event handler that fails MUST be retried, and handling MUST be safe when the same
  event is delivered more than once.

**Architecture conventions**

- **FR-013**: Domain rules MUST live in a domain layer that has no dependency on storage, transport or
  framework concerns.
- **FR-014**: Identifiers of domain entities MUST be distinct types, so that one entity's identifier
  cannot be passed where another entity's is expected.
- **FR-015**: Backend operations MUST validate input at the boundary and report invalid input as a
  structured error that names each invalid field.
- **FR-016**: Backend errors MUST be returned in one consistent, documented error format.
- **FR-017**: The backend MUST publish a browsable, machine-readable description of its operations.
- **FR-018**: The backend MUST log through structured logging and MUST NOT log secrets, tokens or
  personal data.
- **FR-019**: Secrets, tokens and connection strings MUST NOT be committed to the repository. Local
  development values are supplied by the local orchestration.

**Testing**

- **FR-020**: The repository MUST contain a unit-test suite that runs with no external services.
- **FR-021**: The repository MUST contain an integration-test suite that provisions its own isolated,
  disposable infrastructure and removes it afterwards.
- **FR-022**: Integration tests MUST cover the sign-in round trip (an authenticated call succeeds and
  an anonymous call is refused), the durable process and the sample event round trip.
- **FR-023**: The whole test suite MUST run with one documented command.

### Key Entities

- **Account**: The identity a person signs in with (see CONTEXT.md). In this feature it is only read
  from the identity provider: its identifier, display name and whether it is a Platform Operator.
  The backend stores no Account data yet.
- **Sample durable process**: A demonstration process with an identifier, a requested delay, a status
  (running or completed) and a completion time. It exists only to prove the durable-process
  plumbing and may be removed once a real Lifecycle process (such as the Payment window) replaces it.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A developer new to the repository goes from a fresh clone to a fully running system in
  under 15 minutes, following only the README. The time to download prerequisites and container
  images is excluded.
- **SC-002**: After the first run, the start command brings every component to healthy within
  2 minutes.
- **SC-003**: An Account goes from the web app's home page to signed in, with its Account details
  shown, in under 30 seconds.
- **SC-004**: 100% of protected backend operations refuse anonymous calls, verified by automated
  tests.
- **SC-005**: The unit-test suite finishes in under 30 seconds on a typical developer laptop with
  networking disabled.
- **SC-006**: The full test suite, integration tests included, finishes in under 5 minutes and leaves
  no running infrastructure behind.
- **SC-007**: In 10 consecutive runs of the restart scenario in User Story 4, the sample durable
  process completes exactly once in every run.
- **SC-008**: Running the test suite 10 times in a row with no code changes gives the same result
  every time (no flaky tests).

## Assumptions

- **Requested technology stack** (from the user's request; constrains `/speckit-plan`, not this
  spec's requirements): .NET 10, Aspire for local orchestration, FastEndpoints for backend operations,
  EF Core for persistence, Vogen for typed identifiers, DDD layering, Temporal for durable processes,
  Mapster for mapping, Dapr for asynchronous messaging, xUnit v3 with NSubstitute and Shouldly for
  tests, and a React + Vite web app. "keyload" in the request is read as **Keycloak**, which is
  already pinned in `Directory.Packages.props`, and "nsubtitube" as **NSubstitute**.
- **Dependency changes to `Directory.Packages.props`** (approved by the user on 2026-11-04):
  **Mapster** replaces **Mapperly**, reversing #113. **Dapr** replaces **Wolverine** (all
  `WolverineFx*` packages). **NSubstitute** is added. **Shouldly** stays. **Redis** stays as pinned,
  because the user did not ask to remove it.
- The database is the PostgreSQL instance already pinned in `Directory.Packages.props`, run by the
  local orchestration.
- The scope is local development only. Cloud deployment, CI pipelines and production hardening are
  separate features.
- The web app is a single-page app. Its styling and visual design are minimal until a dedicated UI
  feature.
- Account registration and profile management are handled by the identity provider's own pages. The
  skeleton only needs seeded development Accounts.
- No reference domain slice is included (decided 2026-11-04). The first real Marketplace feature
  sets the domain conventions (DDD layering, typed identifiers, mapping, validation) and its tests.
- Merchant ownership, Shops and all Catalog, Buying, Shipping and Money concepts in CONTEXT.md are out
  of scope.
- Developers have a container runtime and the .NET and Node.js toolchains installed. The README lists
  the exact versions.
- Domain terms follow CONTEXT.md (for example, Account rather than "User").
