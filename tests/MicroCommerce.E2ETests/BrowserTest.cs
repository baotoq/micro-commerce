namespace MicroCommerce.E2ETests;

/// <summary>
/// Base class for every browser test: each test gets its own browser context and page, so tests
/// never share cookies or storage (FR-014). Tracing starts only after sign-in, never while the
/// Keycloak password is typed, because a trace records every <c>fill</c> value (research R6).
/// </summary>
public abstract class BrowserTest(MarketplaceFixture fixture) : IAsyncLifetime
{
    private const float DefaultTimeout = 10_000;

    private readonly List<TracedPage> _pages = [];

    protected MarketplaceFixture Fixture { get; } = fixture;

    protected TestAccounts Accounts => Fixture.Accounts;

    /// <summary>The test's main page.</summary>
    protected IPage Page => Main.Page;

    protected IBrowserContext Context => Main.Context;

    private TracedPage Main => _pages[0];

    protected static CancellationToken Ct => TestContext.Current.CancellationToken;

    public virtual async ValueTask InitializeAsync()
    {
        SetDefaultExpectTimeout(DefaultTimeout);
        _pages.Add(await NewPageAsync("main"));
    }

    public virtual async ValueTask DisposeAsync()
    {
        foreach (var page in _pages)
        {
            // Evidence on failure is added by T035; until then traces are discarded.
            if (page.Tracing) await page.Context.Tracing.StopAsync();
            await page.Context.DisposeAsync();
        }
    }

    /// <summary>Opens <paramref name="path"/> signed out, tracing from the start.</summary>
    protected async Task<IPage> VisitAsync(string path)
    {
        await StartTracingAsync(Main);
        await Page.GotoAsync(path);
        return Page;
    }

    /// <summary>Signs the main page in through the identity provider's real sign-in page (FR-007).</summary>
    protected Task SignInAsync(TestAccount account) => SignInAsync(Main, account);

    /// <summary>Another signed-out context, for "a visitor now sees…" steps. Traced from the start.</summary>
    protected async Task<IPage> NewVisitorContextAsync()
    {
        var page = await NewPageAsync($"visitor-{_pages.Count}");
        _pages.Add(page);
        await StartTracingAsync(page);
        return page.Page;
    }

    /// <summary>Another context, signed in as <paramref name="account"/>. Traced from after sign-in.</summary>
    protected async Task<IPage> NewSignedInContextAsync(TestAccount account)
    {
        var page = await NewPageAsync($"signed-in-{_pages.Count}");
        _pages.Add(page);
        await SignInAsync(page, account);
        return page.Page;
    }

    private async Task SignInAsync(TracedPage traced, TestAccount account)
    {
        // A trace started before this point would capture the password typed below.
        traced.Tracing.ShouldBeFalse("Tracing must start only after the Keycloak password is submitted (research R6).");

        var page = traced.Page;
        await page.GotoAsync("/");
        await page.GetByRole(AriaRole.Button, new() { Name = "Sign in" }).ClickAsync();

        // The real Keycloak page, not a bypass (FR-007).
        await Expect(page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("^" + System.Text.RegularExpressions.Regex.Escape(Fixture.KeycloakUrl)));
        await page.GetByLabel("Username or email").FillAsync(account.Username);
        await page.GetByLabel("Password", new() { Exact = true }).FillAsync(account.Password);
        await page.GetByRole(AriaRole.Button, new() { Name = "Sign In" }).ClickAsync();

        await Expect(page.GetByRole(AriaRole.Button, new() { Name = "Sign out" })).ToBeVisibleAsync();
        await Expect(page.GetByText(account.DisplayName, new() { Exact = true })).ToBeVisibleAsync();

        await StartTracingAsync(traced);
    }

    private async Task<TracedPage> NewPageAsync(string label)
    {
        var context = await Fixture.Browser.NewContextAsync(new()
        {
            BaseURL = Fixture.WebUrl,
            ViewportSize = new() { Width = 1280, Height = 800 },
        });
        context.SetDefaultTimeout(DefaultTimeout);
        return new TracedPage(label, context, await context.NewPageAsync());
    }

    private static async Task StartTracingAsync(TracedPage page)
    {
        if (page.Tracing) return;
        await page.Context.Tracing.StartAsync(new() { Screenshots = true, Snapshots = true, Sources = true });
        page.Tracing = true;
    }

    private sealed class TracedPage(string label, IBrowserContext context, IPage page)
    {
        public string Label { get; } = label;

        public IBrowserContext Context { get; } = context;

        public IPage Page { get; } = page;

        public bool Tracing { get; set; }
    }
}
