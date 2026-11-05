using System.Net;

namespace MicroCommerce.IntegrationTests;

/// <summary>One command starts every resource, and each one reports healthy (FR-001 to FR-004).</summary>
public class StartupTests(AppHostFixture fixture)
{
    [Theory]
    [InlineData("postgres")]
    [InlineData("micro-commerce-db")]
    [InlineData("redis")]
    [InlineData("temporal")]
    [InlineData("keycloak")]
    [InlineData("storage")]
    [InlineData("blobs")]
    [InlineData("apiservice")]
    [InlineData("web")]
    public async Task Resource_becomes_healthy(string resource)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cts.CancelAfter(TimeSpan.FromMinutes(2));

        // A resource without a health check counts as healthy once it is running.
        await fixture.App.ResourceNotifications.WaitForResourceHealthyAsync(resource, cts.Token);
    }

    [Fact]
    public async Task Api_health_endpoint_is_ok()
    {
        var response = await fixture.ApiClient.GetAsync("/health", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Web_app_serves_its_home_page()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.App.ResourceNotifications.WaitForResourceHealthyAsync("web", ct);
        using var web = fixture.App.CreateHttpClient("web", "http");

        var response = await web.GetAsync("/", ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("text/html");
        (await response.Content.ReadAsStringAsync(ct)).ShouldContain("<div id=\"root\">");
    }
}
