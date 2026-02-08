using MediatR;
using MicroCommerce.ApiService.Common.Messaging;

namespace MicroCommerce.ApiService.Features.Messaging.Application;

public sealed record RetryDeadLetterMessageCommand(
    string QueueName,
    long SequenceNumber) : IRequest;

public sealed class RetryDeadLetterMessageCommandHandler
    : IRequestHandler<RetryDeadLetterMessageCommand>
{
    private readonly IDeadLetterQueueService _dlqService;

    public RetryDeadLetterMessageCommandHandler(IDeadLetterQueueService dlqService)
    {
        _dlqService = dlqService;
    }

    public async Task Handle(
        RetryDeadLetterMessageCommand request,
        CancellationToken cancellationToken)
    {
        await _dlqService.RetryDeadLetterAsync(
            request.QueueName, request.SequenceNumber, cancellationToken);
    }
}
