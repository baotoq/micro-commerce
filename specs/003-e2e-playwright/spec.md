# Feature Specification: Browser End-to-End Tests

**Feature Branch**: `003-e2e-playwright`

**Created**: 2026-11-05

**Status**: Draft

**Input**: User description: "add e2e playwright"

## Overview

Today the web app is tested only in isolation: component tests mock the sign-in, and backend tests
call the API directly. Nothing checks that a real person, in a real browser, can sign in through the
identity provider and complete a Marketplace journey across the web app, the API and the
infrastructure behind it. Feature 001 deferred browser end-to-end tests and left browser sign-in as a
manual quickstart check.

This feature adds an automated browser end-to-end suite. It drives the web app the way an Account
would, against the whole Marketplace started by the local orchestration with throwaway resources,
and covers the journeys feature 002 (Product catalog) delivered for each role: Buyer, Merchant owner
and Platform Operator.

The people who use this feature are the developers and reviewers of MicroCommerce. The value is
confidence that the Marketplace works end to end before a pull request is merged.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - A developer runs the browser suite with one command (Priority: P1)

A developer on a clean checkout runs one documented command. It starts the whole Marketplace with
throwaway resources, opens a real browser, signs in through the identity provider and checks that a
signed-in Account sees itself in the web app. When the run ends, every resource it started is gone,
and the developer gets a pass or fail result with a report.

**Why this priority**: Every other story needs a suite that can start the Marketplace, sign in and
clean up. Browser sign-in is also the one flow that is checked by hand today.

**Independent Test**: On a machine with only the documented prerequisites, run the command with no
Marketplace already running. Confirm that the sign-in check passes, a report is produced, and no
containers or data from the run are left behind. Run it a second time and confirm the same result.

**Acceptance Scenarios**:

1. **Given** a clean checkout and the documented prerequisites, **When** the developer runs the
   suite command, **Then** the Marketplace starts with throwaway resources, the tests run in a real
   browser, and the command exits with success when all tests pass.
2. **Given** the suite is running, **When** a test signs in as one of the development Accounts,
   **Then** it signs in through the identity provider's own sign-in page, not by bypassing it.
3. **Given** the developer already has the Marketplace running for day-to-day work, **When** they run
   the suite, **Then** the suite does not read, change or delete that environment's data.
4. **Given** a run has finished, passed or failed, **When** the developer checks the machine,
   **Then** no resources or data started by the run remain.
5. **Given** a prerequisite is missing (for example the container runtime is not running),
   **When** the developer runs the suite, **Then** it stops before starting the Marketplace
   or opening a browser, and every test reports the same message naming the missing prerequisite.

---

### User Story 2 - The suite covers the Buyer's browsing journey (Priority: P1)

The suite checks, in the browser, that a Buyer can find and open Products across Merchants: the
Marketplace home lists Published Products, a search narrows them, a Product page shows its Variants,
Availability and images, and its Shop page lists that Merchant's Products. It also checks that
Draft and Removed Products are not shown to Buyers.

**Why this priority**: Browsing is the public face of the Marketplace and the journey most visitors
take. A regression here is visible to everyone.

**Independent Test**: With test data that has one Published, one Draft and one Removed Product from
two Merchants, run only the Buyer tests and confirm each acceptance scenario below.

**Acceptance Scenarios**:

1. **Given** Published Products from two Merchants, **When** a visitor opens the Marketplace home,
   **Then** both Merchants' Products are listed.
2. **Given** a Product named with a distinctive word, **When** the visitor searches for that word,
   **Then** the Product appears in the results.
3. **Given** a Published Product with Variants and images, **When** the visitor opens its page,
   **Then** they see its name, Shop name, cover image, each Variant's price and Availability, and
   never its stock.
4. **Given** a Product page, **When** the visitor follows the link to its Shop, **Then** the Shop page
   lists that Merchant's Published Products.
5. **Given** a Draft Product and a Removed Product, **When** a visitor browses, searches or opens
   their address directly, **Then** neither is shown.

---

### User Story 3 - The suite covers the Merchant's selling journey (Priority: P1)

The suite checks, in the browser, that an Account can open a Merchant and list a Product: it opens a
Shop, creates a Draft Product in the Merchant Centre with Options and Variants, adds images, publishes
it, and then sees it on the Marketplace as a Buyer would.

**Why this priority**: Without this journey nothing can be sold. It also exercises the most
moving parts at once: sign-in, forms, image upload, storage and the public catalog.

**Independent Test**: Sign in as an Account with no Merchant, run only the Merchant tests, and
confirm the Product it lists appears on the Marketplace.

**Acceptance Scenarios**:

1. **Given** a signed-in Account without a Merchant, **When** it opens a Shop with a valid Shop name
   and Pickup Address, **Then** it reaches the Merchant Centre and its Shop page is reachable.
2. **Given** a Merchant owner in the Merchant Centre, **When** it creates a Product with two Options
   and sets each Variant's price and stock, **Then** the Product is saved as Draft with those
   Variants.
3. **Given** a Draft Product, **When** the Merchant owner uploads two images and makes the second the
   cover, **Then** the Product shows the second image first.
4. **Given** a complete Draft Product, **When** the Merchant owner publishes it, **Then** it appears
   on the Marketplace and on the Shop page for a signed-out visitor.
5. **Given** a form with a missing required field, **When** the Merchant owner submits it, **Then**
   the browser shows an error naming that field and nothing is saved.

---

### User Story 4 - The suite covers the Platform Operator's journey (Priority: P2)

The suite checks, in the browser, that a Platform Operator can maintain the Category tree and remove
a prohibited Product, and that a removed Product disappears for Buyers. It also checks that an
Account without the Platform Operator role cannot reach that area.

**Why this priority**: Platform Operator actions are rarer than browsing or selling, but a broken
role check is a security defect, so the refusal path is worth checking in the browser too.

**Independent Test**: Run only the Platform Operator tests, signed in as the development Platform
Operator and as a Buyer, and confirm each acceptance scenario below.

**Acceptance Scenarios**:

1. **Given** a signed-in Platform Operator, **When** they add a Category under an existing one,
   **Then** it appears in the tree and can be chosen for a Product.
2. **Given** a Published Product, **When** a Platform Operator removes it, **Then** a visitor no longer
   sees it on the Marketplace, in search or on its Shop page.
3. **Given** an Account that is not a Platform Operator, **When** it opens the Platform Operator area,
   **Then** it is refused and sees no Platform Operator controls.

---

### User Story 5 - A developer diagnoses a failing browser test (Priority: P2)

When a browser test fails, the developer can see why without re-running it: the report names the
failing step and includes what the browser showed at the moment of failure and a replayable record
of the test's steps.

**Why this priority**: Browser tests that fail without evidence get ignored or retried. The
constitution treats a flaky test as a defect, so a failure must be diagnosable from one run.

**Independent Test**: Make one assertion fail on purpose, run the suite, and confirm the report
names the failing step and includes a screenshot and a step-by-step record for that test only.

**Acceptance Scenarios**:

1. **Given** a test that fails, **When** the run ends, **Then** the report names the test and the
   failing step, and includes a screenshot and a replayable record of that test.
2. **Given** all tests pass, **When** the run ends, **Then** the report lists them as passed and no
   failure evidence is kept.
3. **Given** a developer working on one journey, **When** they ask to run a single test or a single
   role's tests, **Then** only those tests run.

---

### Edge Cases

- **Test data collisions**: Tests that create Merchants or Products use names unique to the run, so
  tests can run in any order, and in parallel, without one test seeing another's data as its own.
  Shop names are unique ignoring case, so a fixed Shop name would make reruns fail.
- **One Merchant per Account**: An Account owns at most one Merchant, so the "open a Shop" test needs
  an Account that owns none. It must not depend on another test having not run first.
- **Slow startup**: The Marketplace takes a while to start. The suite waits until every component
  reports healthy before the first test, and a component that never becomes healthy fails the run
  with its name, not a page timeout.
- **Port conflicts**: Browser sign-in needs the identity provider and web app on the addresses its
  configuration expects. If those ports are already taken (for example by the developer's running
  environment), the suite stops with a clear message instead of signing in to the wrong one.
- **Asynchronous updates**: Some changes, such as a removal showing on the Marketplace, may not be
  visible on the very next page load. Tests wait for the expected state up to a bounded time; they
  never use fixed sleeps.
- **Secrets**: Account passwords are generated for each run, or read from the local orchestration's
  secrets, and tokens are issued at run time. Neither may appear in the test code, the report, failure
  evidence or logs.
- **Interrupted run**: If the developer stops a run part-way, the next run still starts from a clean
  state.

## Requirements *(mandatory)*

### Functional Requirements

**Running the suite**

- **FR-001**: The repository MUST provide one documented command that starts the whole Marketplace
  with throwaway resources, runs the browser suite and returns a success or failure exit status.
- **FR-002**: The suite MUST NOT use, change or depend on a developer's running Marketplace or its
  persistent data.
- **FR-003**: The suite MUST wait until every Marketplace component reports healthy before running
  tests, and MUST fail with the unhealthy component's name if it does not become healthy in time.
- **FR-004**: Every resource and all data a run starts MUST be removed when the run ends, whether it
  passed or failed. After an interrupted run, the next run MUST still start clean.
- **FR-005**: The suite MUST check its prerequisites at start and stop with a message naming any that
  are missing.
- **FR-006**: The documentation MUST list the prerequisites and how to run the whole suite, one
  role's tests and a single test.

**Signing in**

- **FR-007**: Tests MUST sign in through the identity provider's real sign-in page, as the
  development Buyer, Merchant owner and Platform Operator Accounts, or as an Account created for the
  test.
- **FR-008**: Tests MAY reuse a signed-in session across tests of the same role, but at least one
  test MUST complete the full sign-in through the browser on every run.
- **FR-009**: Account passwords MUST be generated for each run or read from the local orchestration's
  secrets at run time. Passwords and tokens MUST NOT be written in tests, configuration files,
  reports, failure evidence or logs.

**Coverage**

- **FR-010**: The suite MUST include a browser test for the sign-in check (User Story 1, scenario 2)
  and for every acceptance scenario in User Stories 2 to 4 of this spec.
- **FR-011**: The suite MUST include at least one authorisation refusal checked in the browser: an
  Account without the Platform Operator role is refused the Platform Operator area (User Story 4,
  scenario 3).
- **FR-012**: Tests MUST interact with the web app the way a person would: through visible labels,
  roles and text, not through internal page structure.
- **FR-013**: Tests and their names MUST use the glossary's terms (Account, Merchant, Shop, Product,
  Variant, Availability, Platform Operator) and none it lists under _Avoid_.

**Isolation and reliability**

- **FR-014**: Each test MUST create or select its own data and MUST NOT depend on another test having
  run, or on the order tests run in.
- **FR-015**: Data created by tests MUST use names unique to the run.
- **FR-016**: Tests MUST wait for an expected state up to a bounded time and MUST NOT use fixed
  delays.
- **FR-017**: The suite MUST NOT retry a failed test to make it pass. A test that fails intermittently
  is fixed or removed.

**Reporting**

- **FR-018**: Each run MUST produce a report listing every test with its result.
- **FR-019**: For each failed test, the report MUST include the failing step, a screenshot of the
  browser at the failure and a replayable step-by-step record. Passed tests MUST NOT keep this
  evidence.
- **FR-020**: Reports and failure evidence MUST be written to a location excluded from version
  control.

**Scope of the browser**

- **FR-021**: The suite MUST run in one desktop browser engine, at a desktop window size.

### Key Entities

- **Browser test**: One automated check of one acceptance scenario, run in a real browser against the
  whole Marketplace. It belongs to one role's group (Buyer, Merchant owner, Platform Operator) or to
  the sign-in group.
- **Test run**: One execution of the suite: the throwaway Marketplace it starts, the tests it runs and
  the report it produces.
- **Test report**: The result of a test run: every test's outcome and, for failures, the failing step,
  screenshot and step-by-step record.
- **Development Account**: One of the seeded Accounts (Buyer, Merchant owner, Platform Operator), or an
  Account created for a test. Its password is generated for the run and never written down.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A developer with only the documented prerequisites runs the full suite from a clean
  checkout with one command and no other manual step.
- **SC-002**: The full suite, including starting and stopping the Marketplace, finishes in under 10
  minutes on a typical developer machine.
- **SC-003**: The sign-in check and every acceptance scenario in User Stories 2 to 4 map to at least
  one browser test (100% coverage of the listed scenarios).
- **SC-004**: Ten consecutive runs of the full suite on an unchanged codebase all pass, with no test
  passing only on retry.
- **SC-005**: After a passed or failed run, zero resources or data started by the run remain, and
  a run after an interrupted one passes.
- **SC-006**: For every failed test, a developer can identify the failing step from the report alone,
  without re-running the test.
- **SC-007**: The manual browser sign-in check in the quickstart is replaced by the automated suite.

## Assumptions

- **Tool**: The user asked for Playwright as the browser test tool. It is a new dependency and is not
  in the constitution's approved stack, so the plan must get it approved and amend the Technology
  Constraints, as feature 002 did for object storage.
- **Who uses it**: The suite is for developers and reviewers. It is not shown to Buyers or Merchants
  and changes no Marketplace behaviour.
- **Where it runs**: The suite runs on developer machines. The repository has no continuous
  integration pipeline today, so running it in one is out of scope; the suite is written so a later
  feature can run it there unchanged.
- **Starting the Marketplace**: The suite reuses the local orchestration's existing throwaway mode
  (ephemeral resources with fixed ports for browser sign-in), the same way the integration tests
  start the Marketplace.
- **Journeys covered**: Only what exists today: browser sign-in (feature 001) and the Product catalog
  (feature 002). Cart, Checkout, Payment and Orders do not exist yet; each later feature adds its own
  browser tests.
- **Browser scope**: One desktop browser engine at a desktop window size. Other browsers, mobile
  sizes, visual comparison against reference screenshots, accessibility audits and performance
  measurement are out of scope.
- **Test pyramid**: Browser tests cover user journeys, not every rule. Validation details, edge cases
  of business rules and authorisation for every operation stay in the unit, functional and
  integration tests, which remain the main safety net.
- **Seed data**: The development Accounts and starter Category tree from features 001 and 002 are
  available in the throwaway Marketplace. Tests that need more data create it themselves.
