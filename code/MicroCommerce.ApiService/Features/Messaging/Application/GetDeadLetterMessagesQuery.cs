using MediatR;
using MicroCommerce.ApiService.Common.Messaging;

namespace MicroCommerce.ApiService.Features.Messaging.Application;

public sealed record GetDeadLetterMessagesQuery(
    string? QueueName = null,
    int MaxMessages = 20) : IRequest<GetDeadLetterMessagesResponse>;

public sealed record GetDeadLetterMessagesResponse(
    IReadOnlyList<DeadLetterMessageDto> Messages,
    IReadOnlyList<string> QueueNames);

public sealed class GetDeadLetterMessagesQueryHandler
    : IRequestHandler<GetDeadLetterMessagesQuery, GetDeadLetterMessagesResponse>
{
    private readonly IDeadLetterQueueService _dlqService;

    public GetDeadLetterMessagesQueryHandler(IDeadLetterQueueService dlqService)
    {
        _dlqService = dlqService;
    }

    public async Task<GetDeadLetterMessagesResponse> Handle(
        GetDeadLetterMessagesQuery request,
        CancellationToken cancellationToken)
    {
        var queueNames = await _dlqService.GetQueueNamesAsync(cancellationToken);

        List<DeadLetterMessageDto> messages;

        if (!string.IsNullOrWhiteSpace(request.QueueName))
        {
            var result = await _dlqService.PeekDeadLettersAsync(
                request.QueueName, request.MaxMessages, cancellationToken);
            messages = result.ToList();
        }
        else
        {
            // Peek from all known queues and combine
            messages = [];
            foreach (var queue in queueNames)
            {
                var result = await _dlqService.PeekDeadLettersAsync(
                    queue, request.MaxMessages, cancellationToken);
                messages.AddRange(result);
            }
        }

        return new GetDeadLetterMessagesResponse(messages, queueNames);
    }
}
