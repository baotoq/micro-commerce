// One throwaway Marketplace and one browser for the whole run; each test gets its own browser context.
[assembly: AssemblyFixture(typeof(MicroCommerce.E2ETests.MarketplaceFixture))]

// Test classes run in parallel, at most 4 at a time (research R9): see xunit.runner.json.
