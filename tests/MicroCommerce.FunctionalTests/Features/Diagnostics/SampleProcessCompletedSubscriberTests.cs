using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using MicroCommerce.ApiService.Features.Diagnostics;
using MicroCommerce.ApiService.Infrastructure.Data;
using MicroCommerce.Domain.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

        var errorsBefore = DatabaseErrorCount();

        (await dapr.SendAsync(CloudEvent(data), ct)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await dapr.SendAsync(CloudEvent(data), ct)).StatusCode.ShouldBe(HttpStatusCode.OK);

        (await InboxRowsAsync(data.EventId, ct)).ShouldBe(1);
        // An expected duplicate is not a failure, so it must not show up as a database error.
        DatabaseErrorCount().ShouldBe(errorsBefore);

        var process = await App.ClientFor(TestActor.PlatformOperator())
            .GetFromJsonAsync<SampleProcessResponse>($"/api/diagnostics/sample-processes/{id}", ct);
        process!.CompletionEventReceivedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task Failed_delivery_returns_500_and_the_retry_is_recorded_once()
    {
        var ct = TestContext.Current.CancellationToken;
        var id = await SeedCompletedProcessAsync(ct);
        var data = new SampleProcessCompletedV1($"sample-process-completed:{id}", id, DateTimeOffset.UtcNow);
        var dapr = DaprClient(App.DaprApiToken);
        App.InboxFaults.FailNextDelivery(data.EventId);

        var failed = await dapr.SendAsync(CloudEvent(data), ct);

        failed.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        (await InboxRowsAsync(data.EventId, ct)).ShouldBe(0);

        (await dapr.SendAsync(CloudEvent(data), ct)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await dapr.SendAsync(CloudEvent(data), ct)).StatusCode.ShouldBe(HttpStatusCode.OK);
        (await InboxRowsAsync(data.EventId, ct)).ShouldBe(1);
    }

    [Fact]
    public async Task Event_without_data_is_a_validation_problem()
    {
        var ct = TestContext.Current.CancellationToken;
        using var request = new HttpRequestMessage(HttpMethod.Post, EventRoute)
        {
            Content = new StringContent("""{"specversion":"1.0","id":"x","type":"t","source":"s"}""", Encoding.UTF8, "application/cloudevents+json"),
        };

        var response = await DaprClient(App.DaprApiToken).SendAsync(request, ct);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.ShouldBe("application/problem+json");
        using var problem = JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));
        problem.RootElement.GetProperty("errors").TryGetProperty("data", out _).ShouldBeTrue();
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

    private int DatabaseErrorCount() =>
        App.Logs.Entries.Count(e => e.Level >= LogLevel.Error && e.Category.StartsWith("Microsoft.EntityFrameworkCore"));

    private async Task<int> InboxRowsAsync(string messageId, CancellationToken ct)
    {
        await using var scope = App.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await db.ProcessedEvents.CountAsync(e => e.MessageId == messageId, ct);
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
