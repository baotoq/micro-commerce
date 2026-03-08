using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using MicroCommerce.ApiService.Features.Messaging.Application;
using MicroCommerce.ApiService.Tests.Integration.Fixtures;

namespace MicroCommerce.ApiService.Tests.Integration.Messaging;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public sealed class MessagingEndpointsTests : IntegrationTestBase
{
    public MessagingEndpointsTests(ApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetDeadLetterMessages_AsAuthenticatedUser_ReturnsOk()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/messaging/dead-letters");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        GetDeadLetterMessagesResponse? result = await response.Content.ReadFromJsonAsync<GetDeadLetterMessagesResponse>();
        result.Should().NotBeNull();
        result!.Messages.Should().BeEmpty();
    }

    [Fact]
    public async Task GetDeadLetterMessages_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        HttpClient client = CreateGuestClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/messaging/dead-letters");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RetryDeadLetterMessage_AsAuthenticatedUser_ReturnsNoContent()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/messaging/dead-letters/retry",
            new { QueueName = "test-queue", SequenceNumber = 1L });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task PurgeDeadLetterMessages_AsAuthenticatedUser_ReturnsOkWithCount()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        HttpClient client = CreateAuthenticatedClient(userId);

        // Act
        HttpResponseMessage response = await client.PostAsJsonAsync(
            "/api/messaging/dead-letters/purge",
            new { QueueName = "test-queue" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string body = await response.Content.ReadAsStringAsync();
        using System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(body);
        doc.RootElement.GetProperty("purgedCount").GetInt32().Should().Be(0);
    }
}
