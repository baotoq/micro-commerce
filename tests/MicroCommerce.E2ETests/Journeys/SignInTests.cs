namespace MicroCommerce.E2ETests.Journeys;

/// <summary>
/// Browser sign-in through Keycloak's real page (User Story 1). Replaces the manual check in
/// 001's quickstart, Scenario 2 (SC-007).
/// </summary>
[Trait("Journey", "SignIn")]
public class SignInTests(MarketplaceFixture fixture) : BrowserTest(fixture)
{
    // US1 #2
    [Fact]
    public async Task An_account_signs_in_through_the_identity_provider_and_sees_itself()
    {
        await SignInAsync(Accounts.Buyer);

        await Expect(Page.GetByText("Signed in as")).ToBeVisibleAsync();
        await Expect(Page.GetByText("Bea Buyer", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Platform Operator: no")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Open your Shop" })).ToBeVisibleAsync();
    }

    // US1 #2
    [Fact]
    public async Task A_platform_operator_signs_in_and_sees_the_platform_operator_role()
    {
        await SignInAsync(Accounts.Operator);

        await Expect(Page.GetByText("Olive Operator", new() { Exact = true })).ToBeVisibleAsync();
        await Expect(Page.GetByText("Platform Operator: yes")).ToBeVisibleAsync();
        await Expect(Page.GetByRole(AriaRole.Link, new() { Name = "Platform operations" })).ToBeVisibleAsync();
    }
}
