using MediatR;
using MicroCommerce.ApiService.Common.Messaging;

namespace MicroCommerce.ApiService.Features.Messaging.Application;

public sealed record PurgeDeadLetterMessagesCommand(string QueueName) : IRequest<int>;

public sealed class PurgeDeadLetterMessagesCommandHandler
    : IRequestHandler<PurgeDeadLetterMessagesCommand, int>
{
    private readonly IDeadLetterQueueService _dlqService;

    public PurgeDeadLetterMessagesCommandHandler(IDeadLetterQueueService dlqService)
    {
        _dlqService = dlqService;
    }

    public async Task<int> Handle(
        PurgeDeadLetterMessagesCommand request,
        CancellationToken cancellationToken)
    {
        return await _dlqService.PurgeDeadLettersAsync(
            request.QueueName, cancellationToken);
    }
}
