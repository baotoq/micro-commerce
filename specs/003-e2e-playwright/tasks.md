---

description: "Task list for feature 003: browser end-to-end tests"
---

# Tasks: Browser End-to-End Tests

**Input**: Design documents from `specs/003-e2e-playwright/`

**Prerequisites**: [plan.md](plan.md), [spec.md](spec.md), [research.md](research.md), [data-model.md](data-model.md), [quickstart.md](quickstart.md)

**Tests**: This feature *is* a test suite. Each user story's tasks are the browser tests for its
acceptance scenarios, plus the harness pieces they need. Every test must fail when the behaviour it
checks is broken: change one expected text, confirm the test fails, then revert. That is constitution
III, "tests that fail without it".

**Organization**: Tasks are grouped by user story, so each story can be built and checked on its own.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependency on unfinished tasks).
- **[Story]**: The user story the task belongs to (US1 to US5).

## Conventions used by every task

- **Project**: `tests/MicroCommerce.E2ETests/`, namespace `MicroCommerce.E2ETests`. It inherits
  `tests/Directory.Build.props`, which brings xUnit v3, Shouldly, NSubstitute, `OutputType=Exe` and
  MTP.
- **Locators**: `GetByRole`, `GetByLabel` and `GetByText` only, scoped to a row or list item when names
  repeat (FR-012). No CSS or XPath selectors, and no test IDs.
- **Waiting**: Use only Playwright web-first assertions (`Expect(locator).ToBeVisibleAsync()`,
  `ToHaveTextAsync`, `ToHaveCountAsync`) and auto-waiting actions. Never `WaitForTimeoutAsync`,
  `Task.Delay` or retries (FR-016, FR-017).
- **Data**: Every created name includes `TestRun.Suffix` (FR-015). Test data is arranged through
  `CatalogArrange` (API). The browser only acts and checks, except in US3, where the whole journey goes
  through the browser.
- **Names**: Test classes, methods and helpers use glossary terms: Account, Merchant, Shop, Product,
  Variant, Availability, Category, Platform Operator, Merchant Centre. Never User, Seller, SKU or Admin
  (FR-013). Methods are sentences, for example
  `A_visitor_sees_published_products_from_two_merchants`.
- **Traits**: Each test class carries `[Trait("Journey", "<SignIn|Buyer|Merchant|PlatformOperator>")]`,
  and each test has a `// US<n> #<k>` comment naming its spec scenario (SC-003).
- **Secrets**: Passwords and tokens are never written to `TestContext` output, assertion messages or
  exceptions (FR-009).

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: The dependency, the project, and the AppHost and realm changes the suite needs.

- [X] T001 Amend `.specify/memory/constitution.md` to **1.2.0** (MINOR): the Testing line adds "Browser
  end-to-end tests use Playwright for .NET", with a Sync Impact Report. *(Done during `/speckit-plan`.)*
- [X] T002 Add `<PackageVersion Include="Microsoft.Playwright" Version="1.64.1" />` to the
  `Label="Testing"` item group in `Directory.Packages.props`, in alphabetical order (research R1).
- [X] T003 Create `tests/MicroCommerce.E2ETests/MicroCommerce.E2ETests.csproj`, modelled on
  `tests/MicroCommerce.IntegrationTests/MicroCommerce.IntegrationTests.csproj`.
  - Package references: `Aspire.Hosting.Testing` and `Microsoft.Playwright`.
  - Project reference: `..\..\src\MicroCommerce.AppHost\MicroCommerce.AppHost.csproj`.
  - Global usings: `Aspire.Hosting`, `Aspire.Hosting.ApplicationModel`, `Aspire.Hosting.Testing` and
    `Microsoft.Playwright`.
  - Also add `using static Microsoft.Playwright.Assertions;` so tests can call `Expect(...)`.
- [X] T004 Add `tests/MicroCommerce.E2ETests/MicroCommerce.E2ETests.csproj` to `MicroCommerce.slnx`,
  next to the other test projects, so `dotnet test --solution MicroCommerce.slnx` runs it (research R10).
- [X] T005 [P] Update `src/MicroCommerce.AppHost/AppHost.cs` (research R2, R3):
  - Read `var webPort = builder.Configuration.GetValue<int?>("WebPort");`.
  - Endpoint: if `webPort` has a value, use `web.WithEndpoint("http", e => e.Port = webPort.Value)`.
    Otherwise keep the existing `fixedPorts` branch at 5173.
  - Add `.WithHttpHealthCheck("/")` to the `web` resource, so `web` reports Healthy only once Vite
    serves the page.
  - Update the comments at lines 10–11 and 108 to mention `WebPort` (5174 is the e2e suite's port).
  - Check: `aspire run` still shows `web` on 5173 and Healthy in the dashboard.
- [X] T006 [P] Update the `micro-commerce-web` client in
  `src/MicroCommerce.AppHost/Realms/micro-commerce-realm.json` (research R3):
  - Add `"http://localhost:5174/*"` to `redirectUris`.
  - Add `"http://localhost:5174"` to `webOrigins`.
  - Set `attributes."post.logout.redirect.uris"` to `"http://localhost:5173/*##http://localhost:5174/*"`.
  - Change no other client or user.
- [X] T007 Verify two assumptions from the plan, and record the outcome of each on this task's line.
  1. The `web` health check from T005 turns Healthy in an ephemeral run, `--EphemeralResources=true
     --WebPort=5174`. Check by running the AppHost once with those arguments.
  2. The xUnit reports can be on by default (research R7). Check whether `dotnet test` with MTP honours
     `<TestingPlatformCommandLineArguments>--report-xunit-html --report-xunit-trx</TestingPlatformCommandLineArguments>`
     in the csproj.
     - If it does, keep it in `tests/MicroCommerce.E2ETests/MicroCommerce.E2ETests.csproj`.
     - If not, use a `testconfig.json` in the project if MTP supports one for these options.
     - Otherwise, document the flags in README (T040) and the quickstart.
  - **Outcome (2026-11-05)**:
    - Reports: `<TestingPlatformCommandLineArguments>` in the csproj works with MTP `dotnet test`. The
      HTML and TRX reports are on by default, and they land in the repository-root `TestResults/`
      (git-ignored), not under `bin/`.
    - Web health check: passes. In an ephemeral run every resource, `web` included, became
      Healthy within about 40 s.
    - **Deviation (T005)**: the testing builder randomizes the host port of every proxied endpoint,
      so `web` landed on a random port instead of 5174. When `WebPort` is set, the web endpoint is now
      non-proxied (`Port = TargetPort = WebPort`, `IsProxied = false`), so Vite listens on 5174
      directly. Turning off `DcpPublisher:RandomizePorts` was rejected, because it would also pin
      `apiservice` to its dev port and break FR-002.
  - **Deviation (T010)**: `CollectionBehavior.MaxParallelThreads` is an obsolete-as-error in xUnit
    4.0.1, and its replacement attribute isn't in the referenced packages. The limit of 4 is set in
    `tests/MicroCommerce.E2ETests/xunit.runner.json` (`maxParallelThreads`) instead.

**Checkpoint**: The solution builds with the empty project, and the AppHost serves the web app on 5174
when asked to.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: The harness every journey test uses: start the Marketplace, provide a browser, create
Accounts, sign in and arrange data.

**⚠️ CRITICAL**: No journey test can be written until this phase is complete.

- [X] T008 [P] Create `tests/MicroCommerce.E2ETests/TestRun.cs`: a static class with
  `public static string Suffix { get; } = Convert.ToHexString(RandomNumberGenerator.GetBytes(3)).ToLowerInvariant();`
  (6 hex characters, data-model "Test run") and `public static string Unique(string prefix)`, which
  returns `$"{prefix} {Suffix}-{Interlocked.Increment(ref _n)}"`. It is used for Shop names, Product
  names and usernames (FR-015).
- [X] T009 Create `tests/MicroCommerce.E2ETests/MarketplaceFixture.cs`, an `IAsyncLifetime` that starts
  the AppHost (research R2–R4). Model it on `tests/MicroCommerce.IntegrationTests/AppHostFixture.cs`,
  copying it rather than referencing it.
  1. **Pre-flight checks** (FR-005, before anything starts), each throwing an
     `InvalidOperationException` with exactly this message:
     - `<runtime> info` exits 0, or "Container runtime not running: start <runtime> and run again."
       `<runtime>` is `podman` when the `ASPIRE_CONTAINER_RUNTIME` environment variable is `podman`,
       and `docker` otherwise, matching Aspire's own choice.
     - `dapr --version` exits 0, or "Dapr CLI not installed: run `dapr init --slim`."
     - A `TcpListener` can bind `127.0.0.1:5174`, or "Port 5174 is in use: the e2e suite serves the web
       app there. Free the port and run again."
     Use `System.Diagnostics.Process` with a 30 s timeout for the two commands.
  2. **Random secrets**: Generate one value each (`RandomNumberGenerator`, as in `AppHostFixture.NewSecret`)
     for `BuyerPassword`, `OperatorPassword`, `MerchantPassword`, `KeycloakAdminPassword` and the Dapr
     token. Expose the first four as properties and never log them.
  3. **Start the AppHost**: Set the working directory to `src/MicroCommerce.AppHost` as
     `AppHostFixture` does. Then call `DistributedApplicationTestingBuilder.CreateAsync<Projects.MicroCommerce_AppHost>`
     with `--EphemeralResources=true`, `--WebPort=5174`, `--Parameters:dev-buyer-password=…`,
     `--Parameters:dev-operator-password=…`, `--Parameters:dev-merchant-password=…`,
     `--Parameters:dapr-app-api-token=…` and `--Parameters:keycloak-password=…`. Use the same logging
     filters and resilience handler as `AppHostFixture`.
  4. **Wait for health**: Wait, with one 5-minute `CancellationTokenSource`, for `apiservice`, `keycloak`,
     `temporal` and `web` to be Healthy. Wait for each one separately, so that a timeout throws
     `TimeoutException($"Resource '{name}' did not become healthy within 5 minutes.")` (FR-003).
  5. **Clients and addresses**: Expose `ApiClient` (`App.CreateHttpClient("apiservice", "http")`),
     `KeycloakClient` (`App.CreateHttpClient("keycloak", "http")`) and `WebUrl = "http://localhost:5174"`.
     Read the actual `web` endpoint and assert that its port is 5174.
  6. **Browser**:
     - Install Chromium with `Microsoft.Playwright.Program.Main(["install", "chromium"])`. If it returns
       non-zero, throw "Chromium install failed".
     - Create `IPlaywright` and launch `Browser = await playwright.Chromium.LaunchAsync(new() { Headless = Environment.GetEnvironmentVariable("HEADED") != "1" })`.
  7. **Dispose**, in order: the browser, then Playwright, the HTTP clients and the app. Restore the
     working directory in a `finally`, as `AppHostFixture` does.
- [X] T010 Create `tests/MicroCommerce.E2ETests/AssemblyInfo.cs` with
  `[assembly: AssemblyFixture(typeof(MicroCommerce.E2ETests.MarketplaceFixture))]` and
  `[assembly: CollectionBehavior(MaxParallelThreads = 4)]` (research R9). Depends on T009.
- [X] T011 Create `tests/MicroCommerce.E2ETests/TestAccounts.cs` (research R5, data-model "Test
  Account").
  1. `AdminTokenAsync`: POST `/realms/master/protocol/openid-connect/token` on `KeycloakClient` with
     `grant_type=password`, `client_id=admin-cli`, `username=admin` and
     `password=fixture.KeycloakAdminPassword`. Cache the token for the run.
  2. `CreateAsync(string role)`: POST `/admin/realms/micro-commerce/users` with
     - `username = $"e2e-{role}-{TestRun.Suffix}-{n}"` (lowercase), `enabled = true`
     - `email = $"{username}@micro-commerce.test"`, `emailVerified = true`
     - `firstName = "E2E"`, `lastName = role`
     - `credentials = [{ type = "password", value = <random>, temporary = false }]`
     - `requiredActions = []`
     Expect `201 Created`. Return a `TestAccount(string Username, string Password, string DisplayName)`
     record whose `ToString()` returns only `Username`, so the password never reaches output.
  3. Seeded Accounts as `TestAccount`s, using the fixture's passwords: `Buyer` (`buyer`, display name
     "Bea Buyer") and `Operator` (`operator`). Read the display name in the realm file; it is `Olive …`.
  4. `TokenAsync(TestAccount)`: the password grant with `client_id=micro-commerce-tests`, as in
     `tests/MicroCommerce.IntegrationTests/KeycloakTokens.cs`.
- [X] T012 Create `tests/MicroCommerce.E2ETests/BrowserTest.cs`, an abstract base class implementing
  `IAsyncLifetime` and taking `MarketplaceFixture` through its constructor.
  - **InitializeAsync**: `Context = await fixture.Browser.NewContextAsync(new() { BaseURL = fixture.WebUrl, ViewportSize = new() { Width = 1280, Height = 800 } })`,
    then `Context.SetDefaultTimeout(10_000)`, `SetDefaultExpectationTimeout(10_000)` (FR-016, research
    R9) and `Page = await Context.NewPageAsync()`.
  - **Tracing**: `protected async Task StartTracingAsync()` calls `Context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true, Sources = true })`
    once (guarded by a flag). It is **not** called in `InitializeAsync` (research R6).
  - **Signed-out tests**: `protected Task<IPage> VisitAsync(string path)` starts tracing, then goes to
    `path`.
  - **Extra contexts**: `protected async Task<IPage> NewVisitorContextAsync()` creates another context
    with the same options, starts tracing on it immediately (it never signs in, so R6 holds), and
    registers it in a list. It is used for "a signed-out visitor now sees…" steps.
    `protected Task<IPage> NewSignedInContextAsync(TestAccount account)` creates a context, signs in with
    the T013 flow on that context, and starts tracing after sign-in. Every registered context is
    disposed, and has its evidence saved, like the main one.
  - **DisposeAsync**: Stops tracing (discarding it for now; T035 adds evidence), then closes the context.
  Depends on T009.
- [X] T013 Create `tests/MicroCommerce.E2ETests/SignIn.cs` with
  `protected async Task SignInAsync(TestAccount account)` as a partial or extension of `BrowserTest`
  (research R5, R6, FR-007).
  1. `Page.GotoAsync("/")`, then click `GetByRole(Button, "Sign in")`.
  2. On the Keycloak page, wait for `GetByLabel("Username or email")` (check the label on the realm's
     login page during implementation), then fill it with `account.Username`.
  3. Fill `GetByLabel("Password", new() { Exact = true })` with `account.Password` and click
     `GetByRole(Button, "Sign In")`.
  4. Expect the web app's `GetByRole(Button, "Sign out")` to be visible, and the `Signed in as` value to
     have text `account.DisplayName`.
  5. **Only then** call `StartTracingAsync()`. Tracing must never have been started before step 3.
     Assert this with the guard flag, so that a future edit can't silently start tracing early.
  Depends on T011, T012.
- [X] T014 Create `tests/MicroCommerce.E2ETests/CatalogArrange.cs`, API helpers that arrange data with
  bearer tokens from `TestAccounts.TokenAsync` (research R8, data-model "Arranged Catalog data"). Copy
  the request shapes from `tests/MicroCommerce.IntegrationTests/CatalogSmokeTests.cs`.
  - `OpenMerchantAsync(token)`: POST `/api/me/merchant`. Shop name `TestRun.Unique("E2E Shop")`,
    Pickup Address `12 Le Loi` / `District 1` / `Ho Chi Minh City`. Expect 201. Returns the Merchant ID
    and Shop name.
  - `KitchenCategoryIdAsync()`: `GET /api/marketplace/categories`, then `Home › Kitchen` (starter tree).
  - `CreateProductAsync(token, name, options, variants)`: POST `/api/merchant/products`. Expect 201.
    Returns ID and version.
  - `UploadImageAsync(token, productId)`: multipart POST of `TestImages.Png` (T015). Expect 201.
  - `PublishAsync(token, productId, version)`: POST `/publish`. Expect 200.
  - `RemoveAsync(operatorToken, productId)`: the Platform Operator removal endpoint. Find its route in
    `src/MicroCommerce.ApiService/Features` (002 US7).
  Each helper asserts the status with `ShouldBe(expected, body)`. The body never contains a token.
- [X] T015 [P] Create `tests/MicroCommerce.E2ETests/TestImages.cs` with two small valid PNGs, `Png`
  and `PngAlt`. Copy them from `tests/MicroCommerce.FunctionalTests/TestImages.cs` if they are valid
  decodable PNGs, because the browser must render them. Also provide
  `WriteTempFile(byte[] bytes, string name)` for browser upload with `SetInputFilesAsync`.

**Checkpoint**: The harness compiles, and an empty `[Fact]` derived from `BrowserTest` that calls
`VisitAsync("/")` starts and stops the Marketplace cleanly.

---

## Phase 3: User Story 1 - A developer runs the browser suite with one command (Priority: P1) 🎯 MVP

**Goal**: `dotnet test --project tests/MicroCommerce.E2ETests` starts the throwaway Marketplace, signs
in through Keycloak's real page and cleans up.

**Independent Test**: Quickstart Scenarios 1, 2, 5 and 6.

- [X] T016 [US1] Create `tests/MicroCommerce.E2ETests/Journeys/SignInTests.cs`,
  `[Trait("Journey", "SignIn")]`. Test `An_account_signs_in_through_the_identity_provider_and_sees_itself` (US1 #2):
  - Sign in as `TestAccounts.Buyer`.
  - Expect the `Signed in as` value "Bea Buyer", the `Platform Operator: no` text, and an
    `Open your Shop` link.
  - Assert that the page URL passed through the Keycloak host on the way. Record the Keycloak origin
    with `Page.Request` events, or assert the Keycloak page heading was seen in `SignInAsync`.
- [X] T017 [US1] In the same file, add `A_platform_operator_signs_in_and_sees_the_platform_operator_role` (US1 #2):
  sign in as `TestAccounts.Operator`, then expect `Platform Operator: yes` and a
  `Platform operations` link.
- [X] T018 [US1] Run the US1 checks from quickstart Scenarios 1, 2, 5 and 6. Record pass or fail, with
  one line each, on this task's line:
  - Whole run passes.
  - Runs while `aspire run` is up, and the dev data is unchanged.
  - Docker stopped: one message on every test, and no browser opened.
  - Port 5174 taken: the port message.
  - `docker ps -a` is clean after a passed run, and after a Ctrl+C run within a minute.
  Fix the fixture (T009) for any check that fails.
  - **Outcome (2026-11-05, Podman, Dapr 1.19.1)**:
    - Whole run: passes, 2/2 in 1 min 49 s, including the first-time Chromium download.
    - Wrong expectation: with "Platform Operator: maybe", the test fails with the locator error,
      so it catches the change (constitution III).
    - Unexplained: one mutation attempt right after the interrupt check failed at "0 ms", with no
      output captured. The rerun failed on the assertion as expected.
    - Missing prerequisite: before Dapr was installed, both tests failed at once with "Dapr CLI not
      installed: run `dapr init --slim`".
    - Port 5174 taken: both tests fail at once with the port message, and nothing starts.
    - Clean-up: after a passed run, and within 1 s of a Ctrl+C mid-run, there are no containers,
      volumes, `daprd` or Vite processes left from the run.
    - Alongside `aspire run` (FR-002, quickstart Scenario 2): passes, 2/2 in 45 s, while the dev
      Marketplace served 5173 and Keycloak 8080. Afterwards the dev Product count, Category tree,
      containers and volumes were unchanged, the dev web app still answered, and no e2e containers
      were left. The dev database itself was not inspected, because that needs its secret password.

**Checkpoint**: The MVP is in place. The suite starts, signs in through the browser and cleans up, and
SC-001 and SC-007 can be shown.

---

## Phase 4: User Story 2 - The suite covers the Buyer's browsing journey (Priority: P1)

**Goal**: Browser tests for every US2 scenario.

**Independent Test**: `-- --filter-trait "Journey=Buyer"` passes, and each test fails when its expected
text is changed.

- [X] T019 [US2] Create `tests/MicroCommerce.E2ETests/Journeys/BuyerBrowsingTests.cs`,
  `[Trait("Journey", "Buyer")]`, with a shared private arrange method. Create two fresh Accounts
  (`TestAccounts.CreateAsync("merchant")`), open a Merchant for each, and create for each one Published
  Product in `Home › Kitchen`, with names from `TestRun.Unique("Mug")`. Each test arranges its own data
  (FR-014); no class fixture.
- [X] T020 [US2] Add `A_visitor_sees_published_products_from_two_merchants` (US2 #1). Arrange as T019,
  then `VisitAsync("/")` with no search or filter, because the scenario is about the home listing.
  Expect both Product names to be visible on the first page, and each card's Shop link to show its own
  Shop name.
  - This relies on the home page listing newest first with a page size of 20, and on the whole run
    creating fewer than 20 Products (about 14 today).
  - Add a comment saying so. If the suite grows past that, change the arrange step to put the two
    Products in a Category created for the test, and filter the home page by it.
- [X] T021 [US2] Add `A_visitor_finds_a_product_by_a_distinctive_word` (US2 #2). Arrange one Published
  Product named `TestRun.Unique("Zephyrine teapot")`. Fill `Search Products` with `Zephyrine`, click
  `Search`, and expect exactly that Product name among the results.
- [X] T022 [US2] Add `A_visitor_sees_a_products_variants_availability_and_cover_image_but_never_stock` (US2 #3):
  - Arrange a Product with one Option `Colour` = `Red`, `Blue`. Red: price 120000, stock 5. Blue: price
    150000, stock 2. Upload one image, then publish.
  - Open `/products/{id}`. Expect: the heading is the Product name; `Sold by` links to the Shop name; an
    `img` is visible and is not the `No image` placeholder.
  - The Variant table has an `Availability` column header and no `Stock` header, and shows 5 and 2.
  - Prices are formatted as the web app formats them. Check `src/MicroCommerce.Web/src/marketplace/ProductPage.tsx`.
  - `GetByText("Stock")` has count 0.
- [X] T023 [US2] Add `A_visitor_follows_a_product_to_its_shop_page` (US2 #4). Arrange one Merchant with
  two Published Products. Open one Product page, click the Shop name link in `Sold by`, then expect the
  Shop heading (`h2`) to be the Shop name and both Product names to be listed.
- [X] T024 [US2] Add `A_visitor_never_sees_draft_or_removed_products` (US2 #5):
  - Arrange one Draft Product and one Product that is Published, then removed with
    `CatalogArrange.RemoveAsync` and an Operator token. Both names contain a shared distinctive word.
  - **Search**: searching for the word shows neither name, and shows the web app's empty-result text.
    Check `ProductGrid.tsx` for it.
  - **Shop page**: shows neither name.
  - **Direct address**: `/products/{id}` for each shows the not-found state that `ProductPage.tsx`
    renders, never the Product name.

- **US2 outcome (2026-11-05)**:
  - `Journey=Buyer` passes 5/5 in about 42 s. The whole suite passes 7/7 in about 41 s, twice in a
    row, with no leftovers.
  - Each test was broken once on purpose and failed: wrong Shop link (#1), wrong result count (#2),
    wrong Availability (#3), missing Product (#4), and the removal skipped (#5).
  - **Fix found by that check**: T021 and T024 first passed against the home listing shown *before*
    the search applied. Both now wait for a known Product to appear first, and then for the
    non-matching Product to disappear. T024 also arranges a Published "control" Product, so an empty
    page can't pass for "not shown".

**Checkpoint**: Buyer browsing is covered in the browser.

---

## Phase 5: User Story 3 - The suite covers the Merchant's selling journey (Priority: P1)

**Goal**: Browser tests for every US3 scenario, driven entirely through the browser.

**Independent Test**: `-- --filter-trait "Journey=Merchant"` passes.

- [ ] T025 [US3] Create `tests/MicroCommerce.E2ETests/Journeys/MerchantSellingTests.cs`,
  `[Trait("Journey", "Merchant")]`. Add a private helper, `OpenShopInBrowserAsync()`:
  - Create a fresh Account with `TestAccounts.CreateAsync("merchant")` and sign in.
  - Click `Open your Shop`, fill `Shop name` = `TestRun.Unique("Sunny Mugs")`, `Street`, `District`,
    `Province`, then click `Open your Shop`.
  - Expect the heading `Merchant Centre: {shopName}`.
  Each test calls the helper itself (FR-014).
- [ ] T026 [US3] Add `An_account_opens_a_shop_and_reaches_the_merchant_centre` (US3 #1): call the
  helper, click `View Shop page`, then expect the Shop page heading to be the Shop name.
- [ ] T027 [US3] Add `A_merchant_creates_a_draft_product_with_two_options_and_its_variants` (US3 #2):
  - In the Merchant Centre, click `New Product`. Fill `Name` and choose `Kitchen` in the Category select.
    Check the field labels in `src/MicroCommerce.Web/src/merchant/ProductForm.tsx`.
  - Add two Options: `Option 1 name` = `Colour` with values `Red`, `Blue` (entered through
    `Option 1 new value`), and `Option 2 name` = `Size` with values `S`, `M`.
  - Fill the four Variant rows (`Red / S price`, `Red / S stock`, and so on, labels from the grid), then
    click `Create Product`.
  - Back in `Products`, expect the row with the Product name to show `Draft`. Opening the Product shows
    four Variant rows with the entered prices.
- [ ] T028 [US3] Add `A_merchant_uploads_two_images_and_makes_the_second_the_cover` (US3 #3):
  - Create a Draft Product in the browser, with no Options and the default Variant.
  - In its image manager, call `SetInputFilesAsync` on `Add an image` twice (`Png`, then `PngAlt`), and
    wait for two images.
  - Click `Move left` on the second image.
  - Expect the first image in the list to be the one uploaded second. Compare `src` or alt text; check
    what `ImageManager.tsx` renders.
- [ ] T029 [US3] Add `A_merchant_publishes_a_product_and_a_visitor_sees_it` (US3 #4):
  - Create a complete Draft Product in the browser, with one image, then click `Publish` in its
    `Products` row and expect `Published`.
  - Then, in a visitor context (`NewVisitorContextAsync()`, T012), search for the Product name and
    expect it on the Marketplace and on the Shop page. The base class disposes it and saves its evidence
    on failure.
- [ ] T030 [US3] Add `A_merchant_sees_which_field_is_missing_and_nothing_is_saved` (US3 #5):
  - Open `New Product`, leave `Name` empty and submit with `Create Product`.
  - `Name` has the `required` attribute, so the browser blocks the submit and shows its own message on
    that field. That message is how "an error naming that field" (US3 #5) is checked. Assert both:
    - `Expect(Page.GetByLabel("Name")).ToHaveJSPropertyAsync("validity.valid", false)`.
    - `GetByLabel("Name").EvaluateAsync<string>("e => e.validationMessage")` is not empty.
  - Return to `Products` and expect no new row.
  - Do the same on `Open your Shop` with `Province` empty. Check `ShopForm.tsx` for whether the field
    is `required`.
    - If it is, use the same two assertions on `GetByLabel("Province")`.
    - If not, the API refuses the request. Expect the field error that `ShopForm`'s `Field` renders
      under Province, as text that contains "Province" or the API's message for
      `pickupAddress.province`.
    - Either way, the Account still has no Merchant (`Open your Shop` link still shown).

**Checkpoint**: The Merchant journey is covered end to end, including image storage.

---

## Phase 6: User Story 4 - The suite covers the Platform Operator's journey (Priority: P2)

**Goal**: Browser tests for every US4 scenario, including the authorisation refusal (FR-011).

**Independent Test**: `-- --filter-trait "Journey=PlatformOperator"` passes.

- [ ] T031 [US4] Create `tests/MicroCommerce.E2ETests/Journeys/PlatformOperatorTests.cs`,
  `[Trait("Journey", "PlatformOperator")]`. Add `A_platform_operator_adds_a_child_category` (US4 #1):
  - Sign in as `TestAccounts.Operator`, click `Platform operations` and open the Category tree. Check
    the route in `src/MicroCommerce.Web/src/ops/OpsArea.tsx`.
  - Register `Page.Dialog += (_, d) => d.AcceptAsync(TestRun.Unique("Teaware"))`, because the tree uses
    `window.prompt`.
  - Within the list item whose text starts with `Kitchen`, click `Add child`.
  - Expect the new name inside Kitchen's list item.
  - Then, in `NewSignedInContextAsync(fresh Merchant Account)` (T012), open `New Product`, and expect the new
    Category among the Category select's options. The fresh Account opens its Merchant through
    `CatalogArrange.OpenMerchantAsync` first.
- [ ] T032 [US4] Add `A_platform_operator_removes_a_product_and_visitors_no_longer_see_it` (US4 #2):
  - Arrange a Published Product through the API.
  - Sign in as Operator and open Product moderation. Fill `Product ID` with the ID, click `Look up`,
    then `Remove from the Marketplace`. Accept a confirm dialog if `ProductModeration.tsx` shows one, or
    fill a reason field if it has one; check the file.
  - Expect the status `Removed`.
  - In `NewVisitorContextAsync()` (T012), expect the Product name absent from search and from the Shop
    page, and its direct address to show the not-found state.
- [ ] T033 [US4] Add `An_account_without_the_platform_operator_role_is_refused_the_platform_operator_area` (US4 #3, FR-011):
  - Sign in as `TestAccounts.Buyer`.
  - Expect no `Platform operations` link.
  - Go directly to `/ops`, then expect the refusal text that `OpsArea.tsx` renders for non-operators.
    Read the file for the exact text, and add one if it is missing (see T034).
  - Expect `Add top-level Category` and `Remove from the Marketplace` to have count 0.
- [ ] T034 [US4] **Only if** T031–T033 find a control without an accessible name, or no refusal text
  for non-operators, make the minimal fix in `src/MicroCommerce.Web/src/ops/*.tsx`: a `<label>`,
  `aria-label` or a visible refusal message such as "Only Platform Operators can use this area." Never
  a `data-testid`. Add or extend a Vitest test next to the component for the new text, and run
  `pnpm --dir src/MicroCommerce.Web test`. If nothing is needed, mark this task done with "no change
  needed".

**Checkpoint**: All four journeys are covered, including one browser-level authorisation refusal.

---

## Phase 7: User Story 5 - A developer diagnoses a failing browser test (Priority: P2)

**Goal**: Each failed test leaves a screenshot and a replayable trace, and passed tests leave nothing
(FR-018 to FR-020).

**Independent Test**: Quickstart Scenarios 3 and 4.

- [ ] T035 [US5] Extend `DisposeAsync` in `tests/MicroCommerce.E2ETests/BrowserTest.cs` (research R7).
  Apply the steps below to the main context and to every extra context from T012. Extra contexts get
  the file-name suffix `.visitor-<n>` or `.signed-in-<n>`.
  - Work out `failed = TestContext.Current.TestState?.Result == TestResult.Failed`, and
    `name = $"{TestContext.Current.TestClass!.TestClassName.Split('.').Last()}.{TestContext.Current.TestMethod!.MethodName}"`.
  - Set `dir = Path.Combine(AppContext.BaseDirectory, "TestResults", "e2e")`.
  - **If failed**: save `Page.ScreenshotAsync(new() { Path = $"{dir}/{name}.png", FullPage = true })`.
    If tracing was started:
    1. `Context.Tracing.StopAsync(new() { Path = tmp })`, where `tmp` is a path under `Path.GetTempPath()`.
    2. `TraceRedactor.Redact(tmp)` (T036).
    3. `File.Move(tmp, $"{dir}/{name}.zip", overwrite: true)`.
    If redaction throws, delete `tmp` and keep no trace. An unredacted trace is never left behind
    (constitution IV, research R6). Write both
    paths with `TestContext.Current.SendDiagnosticMessage`, or add them to the test output, so they
    appear in the report.
  - **Otherwise**: `StopAsync()` with no path, so nothing is kept.
  - Wrap evidence capture in try/catch, so a closed page never hides the original failure.
  - Confirm `TestResults/` is git-ignored. It already is, by `.gitignore` line 34.
- [ ] T036 [US5] Create `tests/MicroCommerce.E2ETests/TraceRedactor.cs` (research R6, constitution IV).
  - Add `public static void Redact(string zipPath)`. Open the zip in update mode, and for each entry
    read its bytes.
  - If the bytes decode as strict UTF-8 (`new UTF8Encoding(false, true)`), replace every match of
    `eyJ[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]*` with `REDACTED` and write the entry back.
  - Leave binary entries, such as screenshots, untouched.
  - Add a unit test in the same project, `TraceRedactorTests`, with no `Journey` trait and no
    Marketplace needed. Build a zip with a `.network` entry that holds an `authorization: Bearer <jwt>`
    header and a JSON body with `"access_token":"<jwt>"`. After `Redact`, neither JWT remains and the
    other text is unchanged.
    - This unit test must not use `MarketplaceFixture`. Check whether xUnit v3 starts an assembly
      fixture only for tests that request it. If it starts the fixture for every test, move the test to
      `tests/MicroCommerce.UnitTests`, referencing `TraceRedactor.cs` as a linked file.
- [ ] T037 [US5] Verify that `TestState` is populated by the time `IAsyncLifetime.DisposeAsync` runs for
  the test class instance in xUnit v3 4.0.1. If it isn't, move evidence capture to a
  `BeforeAfterTestAttribute` applied to `BrowserTest`, using xUnit v3's
  `After(MethodInfo, IXunitTest)` signature, and record which approach was used on this task's line.
- [ ] T038 [US5] Run quickstart Scenario 4:
  - Break one expected text in `BuyerBrowsingTests` and run `-- --filter-trait "Journey=Buyer"`.
  - Confirm the `.png` and `.zip` exist for that test only, and that `npx playwright show-trace <zip>`
    replays it.
  - Unzip the trace and `grep -c` for the run's `BuyerPassword`. The count must be 0, and so must the
    count for a fresh Account's password (FR-009, research R6).
  - `grep -rc "eyJ"` over the unzipped trace must also be 0 for every file (no bearer tokens,
    constitution IV).
  - Revert the change, and record the result on this task's line.
- [ ] T039 [US5] Run quickstart Scenario 3: `--filter-trait "Journey=Merchant"` runs only the Merchant
  tests, `--filter-method` runs one test, and `HEADED=1` shows the browser. Record the result.

**Checkpoint**: Failures can be diagnosed from one run.

---

## Phase 8: Polish & Cross-Cutting Concerns

- [ ] T040 [P] Update `README.md`.
  - **Tests table**: add a row. `tests/MicroCommerce.E2ETests` covers the browser journeys (sign-in,
    Buyer browsing, Merchant selling, Platform Operator) against the whole AppHost in Chromium. It needs
    what the integration tests need, plus a free port 5174. Chromium downloads itself on first run.
  - **Commands block**: add `dotnet test --project tests/MicroCommerce.E2ETests` and the
    `--filter-trait "Journey=…"` example.
  - **Debugging**: one paragraph on `HEADED=1`, the HTML report location, `TestResults/e2e/` evidence
    and `npx playwright show-trace`.
  - **Report flags**: add them if T007 found they are not on by default.
- [ ] T041 [P] Update `specs/001-project-skeleton/quickstart.md` Scenario 2: keep the manual steps,
  and add one line saying they are automated by `tests/MicroCommerce.E2ETests/Journeys/SignInTests.cs`
  (SC-007).
- [ ] T042 Coverage check (SC-003): confirm that every row of the mapping table in
  `specs/003-e2e-playwright/data-model.md` has a test with a matching `// US<n> #<k>` comment. Expected
  counts: SignIn ≥ 1, Buyer ≥ 5, Merchant ≥ 5, PlatformOperator ≥ 3. Fix any gap.
- [ ] T043 Glossary check (FR-013, constitution I): grep the new project for `User`, `Seller`, `SKU`,
  `Admin`, `Customer` and `Store`. Exceptions are the Playwright and Keycloak API names, such as
  `admin-cli`, the `/admin/` REST path and Keycloak's own login-page labels. Rename any other hits.
- [ ] T044 Run the full suite and record its duration on this task's line (SC-002: under 10 minutes):
  `dotnet test --project tests/MicroCommerce.E2ETests`.
- [ ] T045 Run `dotnet test --solution MicroCommerce.slnx` and `pnpm --dir src/MicroCommerce.Web test`.
  Everything must pass, because `master` must stay green.
  - The solution run starts the integration-test and e2e AppHosts at the same time. Run it 3 times and
    record the wall time and the result on this task's line.
  - If any run fails only under that combined load, run the test projects one at a time:
    `dotnet test --solution MicroCommerce.slnx --max-parallel-test-modules 1`. Check the option name
    with `dotnet test --help`. Use it in the README's solution command (T040) and record the choice
    here. Never retry.
- [ ] T046 Run the 10-run reliability check (SC-004, quickstart Scenario 7):
  `for i in $(seq 10); do dotnet test --project tests/MicroCommerce.E2ETests || break; done`.
  A failing run is a defect to fix (FR-017), never a retry. Record the result.
- [ ] T047 Re-check the plan's Constitution Check table against the finished code:
  - No secrets in code.
  - Tracing starts only after sign-in.
  - The only new dependency is `Microsoft.Playwright`.
  - Only the planned AppHost, realm and web changes were made.
  Note any deviation in `specs/003-e2e-playwright/plan.md` Complexity Tracking.

---

## Dependencies & Execution Order

### Phase dependencies

- **Setup (Phase 1)**: T002 → T003 → T004. T005 and T006 can run in parallel with T002–T004. T007
  needs T003, T005 and T006.
- **Foundational (Phase 2)**: Needs Phase 1. T008 and T015 can run in parallel. T009 → T010 and T012.
  T011 needs T009. T013 needs T011 and T012. T014 needs T011 and T015.
- **US1 (Phase 3)**: Needs Phase 2. It is the MVP and proves the harness.
- **US2, US3 and US4 (Phases 4–6)**: Each needs Phase 2. They don't depend on each other and use
  different files, so they can be built in parallel. Running them all together is the real check of
  FR-014, which says no test may depend on another.
- **US5 (Phase 7)**: Needs T012 and at least one journey test to break on purpose, which is US2.
- **Polish (Phase 8)**: Needs all stories.

### Within each story

The tests in one story share one file, so they are written one after another. The arrange helpers
from Phase 2 come first.

### Parallel opportunities

- Phase 1: T005 ∥ T006 ∥ (T002 → T003 → T004).
- Phase 2: T008 ∥ T015, then T012 ∥ T011 once T009 is done.
- After Phase 2: `BuyerBrowsingTests.cs` (US2) ∥ `MerchantSellingTests.cs` (US3) ∥
  `PlatformOperatorTests.cs` (US4).
- Phase 8: T040 ∥ T041.

## Parallel Example: after the foundation

```text
Task: "T019–T024 [US2] Buyer browsing tests in tests/MicroCommerce.E2ETests/Journeys/BuyerBrowsingTests.cs"
Task: "T025–T030 [US3] Merchant selling tests in tests/MicroCommerce.E2ETests/Journeys/MerchantSellingTests.cs"
Task: "T031–T033 [US4] Platform Operator tests in tests/MicroCommerce.E2ETests/Journeys/PlatformOperatorTests.cs"
```

## Implementation Strategy

### MVP first (User Story 1)

1. Phase 1, then Phase 2.
2. Phase 3 (US1): one command, a real sign-in, clean-up.
3. **Stop and validate** with quickstart Scenarios 1, 2, 5 and 6. This already replaces the manual
   sign-in check (SC-007).

### Incremental delivery

1. US2 (Buyer) → US3 (Merchant) → US4 (Platform Operator). Run the suite after each one.
2. US5 (evidence) as soon as any journey exists. It is cheap, and it helps with debugging US3 and US4.
3. Polish: README, coverage, glossary, duration, and the 10-run reliability check.

## Notes

- `[P]` tasks touch different files and don't depend on unfinished tasks.
- A test that fails intermittently is fixed or removed, never retried (FR-017, constitution III).
- Commit after each phase. The feature lands as one squash-merged PR from `003-e2e-playwright`.
