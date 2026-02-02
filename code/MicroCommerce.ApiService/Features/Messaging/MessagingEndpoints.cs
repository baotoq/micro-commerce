using MediatR;
using Microsoft.AspNetCore.Mvc;
using MicroCommerce.ApiService.Features.Messaging.Application;

namespace MicroCommerce.ApiService.Features.Messaging;

/// <summary>
/// Messaging module endpoints.
/// Provides admin DLQ management operations.
/// </summary>
public static class MessagingEndpoints
{
    public static IEndpointRouteBuilder MapMessagingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/messaging")
            .WithTags("Messaging")
            .RequireAuthorization();

        group.MapGet("/dead-letters", GetDeadLetterMessages)
            .WithName("GetDeadLetterMessages")
            .WithSummary("List dead-lettered messages across queues")
            .Produces<GetDeadLetterMessagesResponse>();

        group.MapPost("/dead-letters/retry", RetryDeadLetterMessage)
            .WithName("RetryDeadLetterMessage")
            .WithSummary("Retry a specific dead-lettered message")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/dead-letters/purge", PurgeDeadLetterMessages)
            .WithName("PurgeDeadLetterMessages")
            .WithSummary("Purge all dead-lettered messages from a queue")
            .Produces<PurgeDeadLetterMessagesResponse>();

        return endpoints;
    }

    private static async Task<IResult> GetDeadLetterMessages(
        [FromQuery] string? queueName,
        [FromQuery] int? maxMessages,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetDeadLetterMessagesQuery(queueName, maxMessages ?? 20);
        var result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> RetryDeadLetterMessage(
        RetryDeadLetterMessageRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new RetryDeadLetterMessageCommand(request.QueueName, request.SequenceNumber);
        await sender.Send(command, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> PurgeDeadLetterMessages(
        PurgeDeadLetterMessagesRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new PurgeDeadLetterMessagesCommand(request.QueueName);
        var count = await sender.Send(command, cancellationToken);
        return Results.Ok(new PurgeDeadLetterMessagesResponse(count));
    }
}

// Request/Response records for endpoint contracts
public sealed record RetryDeadLetterMessageRequest(string QueueName, long SequenceNumber);

public sealed record PurgeDeadLetterMessagesRequest(string QueueName);

public sealed record PurgeDeadLetterMessagesResponse(int PurgedCount);
