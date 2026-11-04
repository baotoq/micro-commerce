using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Temporalio.Api.Enums.V1;
using Temporalio.Client;

namespace MicroCommerce.IntegrationTests;

/// <summary>
/// The sample durable process end to end: real Keycloak token, real Temporal, and the completion
/// event through the real Dapr sidecar and Redis (FR-011, FR-012, FR-012a, SC-007).
/// </summary>
public class SampleProcessLifecycleTests(AppHostFixture fixture)
{
    private const string Route = "/api/diagnostics/sample-processes";

    private sealed record SampleProcess(
        Guid Id, string Status, int DelaySeconds, DateTimeOffset StartedAt, DateTimeOffset? CompletedAt, DateTimeOffset? CompletionEventReceivedAt);

    [Fact]
    public async Task Process_completes_and_its_event_comes_back_through_dapr()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = await OperatorClientAsync(ct);

        var id = await StartAsync(client, delaySeconds: 3, ct);
        var process = await PollUntilAsync(client, id, p => p.CompletionEventReceivedAt is not null, TimeSpan.FromSeconds(30), ct);

        process.Status.ShouldBe("Completed");
        (process.CompletedAt!.Value - process.StartedAt).ShouldBeGreaterThanOrEqualTo(TimeSpan.FromSeconds(3));
    }

    [Fact]
    [Trait("Category", "Restart")]
    public async Task Process_survives_an_api_restart_and_completes_exactly_once()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = await OperatorClientAsync(ct);

        var id = await StartAsync(client, delaySeconds: 10, ct);
        await Task.Delay(TimeSpan.FromSeconds(2), ct);

        var restart = await fixture.App.ResourceCommands.ExecuteCommandAsync("apiservice", KnownResourceCommands.RestartCommand, ct);
        restart.Success.ShouldBeTrue(restart.Message);
        await fixture.App.ResourceNotifications.WaitForResourceHealthyAsync("apiservice", ct);

        // The restarted API has a new port, so get a fresh client.
        using var restarted = await OperatorClientAsync(ct);
        var process = await PollUntilAsync(restarted, id, p => p.CompletionEventReceivedAt is not null, TimeSpan.FromSeconds(60), ct);

        process.Status.ShouldBe("Completed");
        process.CompletedAt.ShouldNotBeNull();
        (await CompletedActivityCountAsync(id, "CompleteSampleProcess", ct)).ShouldBe(1);
    }

    private async Task<int> CompletedActivityCountAsync(Guid id, string activity, CancellationToken ct)
    {
        var endpoint = fixture.App.GetEndpoint("temporal", "grpc");
        var temporal = await TemporalClient.ConnectAsync(new TemporalClientConnectOptions($"{endpoint.Host}:{endpoint.Port}"));
        var history = await temporal.GetWorkflowHandle($"sample-process-{id}")
            .FetchHistoryAsync(new WorkflowHistoryEventFetchOptions { Rpc = new RpcOptions { CancellationToken = ct } });

        var scheduled = history.Events
            .Where(e => e.EventType == EventType.ActivityTaskScheduled
                && e.ActivityTaskScheduledEventAttributes.ActivityType.Name == activity)
            .Select(e => e.EventId)
            .ToHashSet();
        return history.Events.Count(e => e.EventType == EventType.ActivityTaskCompleted
            && scheduled.Contains(e.ActivityTaskCompletedEventAttributes.ScheduledEventId));
    }

    private async Task<HttpClient> OperatorClientAsync(CancellationToken ct)
    {
        var client = fixture.App.CreateHttpClient("apiservice", "http");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await fixture.OperatorTokenAsync(ct));
        return client;
    }

    private static async Task<Guid> StartAsync(HttpClient client, int delaySeconds, CancellationToken ct)
    {
        var response = await client.PostAsJsonAsync(Route, new { delaySeconds }, ct);
        response.StatusCode.ShouldBe(HttpStatusCode.Accepted);
        return (await response.Content.ReadFromJsonAsync<SampleProcess>(ct))!.Id;
    }

    private static async Task<SampleProcess> PollUntilAsync(
        HttpClient client, Guid id, Func<SampleProcess, bool> done, TimeSpan timeout, CancellationToken ct)
    {
        var deadline = DateTime.UtcNow + timeout;
        while (true)
        {
            try
            {
                var response = await client.GetAsync($"{Route}/{id}", ct);
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var process = (await response.Content.ReadFromJsonAsync<SampleProcess>(ct))!;
                    if (done(process)) return process;
                }
            }
            catch (HttpRequestException)
            {
                // The API can be briefly unreachable while it restarts.
            }

            if (DateTime.UtcNow > deadline) throw new TimeoutException($"Sample process {id} didn't reach the expected state in {timeout}.");
            await Task.Delay(500, ct);
        }
    }
}
