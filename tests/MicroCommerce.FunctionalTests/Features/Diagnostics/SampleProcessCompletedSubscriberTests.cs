using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using MicroCommerce.ApiService.Features.Diagnostics;
using MicroCommerce.ApiService.Infrastructure.Data;
using MicroCommerce.Domain.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MicroCommerce.FunctionalTests.Features.Diagnostics;

/// <summary>
/// The Dapr subscriber endpoints: authenticated with the app API token, and idempotent (FR-012b).
/// </summary>
public class SampleProcessCompletedSubscriberTests(ApiFixture App) : TestBase<ApiFixture>
{
    private const string EventRoute = "/dapr/events/sample-process-completed";

    [Fact]
    public async Task Same_event_delivered_twice_is_recorded_once()
    {
        var ct = TestContext.Current.CancellationToken;
        var id = await SeedCompletedProcessAsync(ct);
        var data = new SampleProcessCompletedV1($"sample-process-completed:{id}", id, DateTimeOffset.UtcNow);
        var dapr = DaprClient(App.DaprApiToken);

        (await dapr.SendAsync(CloudEvent(data), ct)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await dapr.SendAsync(CloudEvent(data), ct)).StatusCode.ShouldBe(HttpStatusCode.OK);

        await using (var scope = App.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            (await db.ProcessedEvents.CountAsync(e => e.MessageId == data.EventId, ct)).ShouldBe(1);
        }

        var process = await App.ClientFor(TestActor.PlatformOperator())
            .GetFromJsonAsync<SampleProcessResponse>($"/api/diagnostics/sample-processes/{id}", ct);
        process!.CompletionEventReceivedAt.ShouldNotBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("wrong-token")]
    public async Task Event_without_the_app_token_is_unauthorized(string? token)
    {
        var data = new SampleProcessCompletedV1($"sample-process-completed:{Guid.NewGuid()}", Guid.NewGuid(), DateTimeOffset.UtcNow);

        var response = await DaprClient(token).SendAsync(CloudEvent(data), TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Subscription_list_names_the_topic_route()
    {
        var ct = TestContext.Current.CancellationToken;

        var response = await DaprClient(App.DaprApiToken).GetAsync("/dapr/subscribe", ct);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync(ct);
        using var document = JsonDocument.Parse(json);
        var subscription = document.RootElement.EnumerateArray().ShouldHaveSingleItem();
        subscription.GetProperty("pubsubname").GetString().ShouldBe("pubsub");
        subscription.GetProperty("topic").GetString().ShouldBe("sample-process-completed");
        subscription.GetProperty("route").GetString().ShouldBe(EventRoute);
    }

    [Fact]
    public async Task Subscription_list_without_the_app_token_is_unauthorized()
    {
        var response = await DaprClient(null).GetAsync("/dapr/subscribe", TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    private HttpClient DaprClient(string? token) => App.CreateClient(c =>
    {
        if (token is not null) c.DefaultRequestHeaders.Add("dapr-api-token", token);
    });

    private static HttpRequestMessage CloudEvent(SampleProcessCompletedV1 data) =>
        new(HttpMethod.Post, EventRoute)
        {
            Content = new StringContent(
                JsonSerializer.Serialize(new
                {
                    specversion = "1.0",
                    id = Guid.NewGuid().ToString(),
                    source = "apiservice",
                    type = "com.dapr.event.sent",
                    topic = SampleProcessCompletedV1.Topic,
                    pubsubname = "pubsub",
                    datacontenttype = "application/json",
                    data,
                }, JsonSerializerOptions.Web),
                Encoding.UTF8,
                "application/cloudevents+json"),
        };

    private async Task<Guid> SeedCompletedProcessAsync(CancellationToken ct)
    {
        var id = SampleProcessId.New();
        var startedAt = DateTimeOffset.UtcNow.AddMinutes(-1);
        var process = SampleProcess.Start(id, TimeSpan.FromSeconds(1), startedAt);
        process.Complete(startedAt.AddSeconds(1));

        await using var scope = App.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.SampleProcesses.Add(process);
        await db.SaveChangesAsync(ct);
        return id.Value;
    }
}
