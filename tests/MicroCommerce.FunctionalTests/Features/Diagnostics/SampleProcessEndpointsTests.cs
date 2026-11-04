using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using MicroCommerce.ApiService.Features.Diagnostics;
using MicroCommerce.ApiService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MicroCommerce.FunctionalTests.Features.Diagnostics;

/// <summary>
/// The sample durable process through the API, with the workflow running on a real Temporal server
/// (FR-011, FR-012, SC-004).
/// </summary>
public class SampleProcessEndpointsTests(ApiFixture App) : TestBase<ApiFixture>
{
    private const string Route = "/api/diagnostics/sample-processes";

    [Fact]
    public async Task Operator_starts_a_process_that_completes_after_its_delay()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = App.ClientFor(TestActor.PlatformOperator());

        var start = await client.PostAsJsonAsync(Route, new { delaySeconds = 1 }, ct);

        start.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        var accepted = (await start.Content.ReadFromJsonAsync<SampleProcessResponse>(ct))!;
        accepted.Status.ShouldBe("Running");
        accepted.DelaySeconds.ShouldBe(1);
        accepted.CompletedAt.ShouldBeNull();
        start.Headers.Location!.ToString().ShouldBe($"{Route}/{accepted.Id}");

        var completed = await PollUntilAsync(client, accepted.Id, p => p.Status == "Completed", TimeSpan.FromSeconds(15), ct);

        completed.StartedAt.ShouldBe(accepted.StartedAt, TimeSpan.FromMilliseconds(1));
        (completed.CompletedAt!.Value - completed.StartedAt).ShouldBeGreaterThanOrEqualTo(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Running_the_create_activity_twice_keeps_one_process()
    {
        var ct = TestContext.Current.CancellationToken;
        var id = Guid.NewGuid();
        var startedAt = DateTimeOffset.UtcNow;

        await using (var scope = App.Services.CreateAsyncScope())
        {
            var activities = scope.ServiceProvider.GetRequiredService<SampleProcessActivities>();
            await activities.CreateSampleProcessAsync(id, TimeSpan.FromSeconds(5), startedAt);
        }
        await using (var scope = App.Services.CreateAsyncScope())
        {
            var activities = scope.ServiceProvider.GetRequiredService<SampleProcessActivities>();
            await activities.CreateSampleProcessAsync(id, TimeSpan.FromSeconds(5), startedAt);
        }

        await using var check = App.Services.CreateAsyncScope();
        var db = check.ServiceProvider.GetRequiredService<AppDbContext>();
        (await db.SampleProcesses.CountAsync(p => p.Id == Domain.Diagnostics.SampleProcessId.From(id), ct)).ShouldBe(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(301)]
    public async Task Delay_outside_1_to_300_seconds_is_a_validation_problem(int seconds)
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await App.ClientFor(TestActor.PlatformOperator()).PostAsJsonAsync(Route, new { delaySeconds = seconds }, ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
        var problem = JsonNode.Parse(await response.Content.ReadAsStringAsync(ct))!;
        problem["errors"]!["delaySeconds"].ShouldNotBeNull();
    }

    [Fact]
    public async Task Start_without_a_token_is_unauthorized()
    {
        var response = await App.Client.PostAsJsonAsync(Route, new { delaySeconds = 1 }, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Start_by_a_buyer_is_forbidden()
    {
        var response = await App.ClientFor(TestActor.Buyer()).PostAsJsonAsync(Route, new { delaySeconds = 1 }, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Get_without_a_token_is_unauthorized()
    {
        var response = await App.Client.GetAsync($"{Route}/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Get_by_a_buyer_is_forbidden()
    {
        var response = await App.ClientFor(TestActor.Buyer()).GetAsync($"{Route}/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Unknown_process_is_not_found()
    {
        var response = await App.ClientFor(TestActor.PlatformOperator()).GetAsync($"{Route}/{Guid.NewGuid()}", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Non_uuid_id_is_a_bad_request()
    {
        var response = await App.ClientFor(TestActor.PlatformOperator()).GetAsync($"{Route}/not-a-uuid", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    /// <summary>The workflow creates the record as its first step, so a 404 right after the 202 means "not yet".</summary>
    internal static async Task<SampleProcessResponse> PollUntilAsync(
        HttpClient client, Guid id, Func<SampleProcessResponse, bool> done, TimeSpan timeout, CancellationToken ct)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (true)
        {
            var response = await client.GetAsync($"{Route}/{id}", ct);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var process = (await response.Content.ReadFromJsonAsync<SampleProcessResponse>(ct))!;
                if (done(process)) return process;
            }
            else
            {
                response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            }

            if (DateTime.UtcNow > deadline) throw new TimeoutException($"Sample process {id} didn't reach the expected state in {timeout}.");
            await Task.Delay(200, ct);
        }
    }
}
