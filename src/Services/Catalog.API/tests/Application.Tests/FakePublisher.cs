using MediatR;

namespace MicroCommerce.Catalog.Application.Tests;

sealed class FakePublisher : IPublisher
{
    public List<INotification> Published { get; } = [];

    public Task Publish(object notification, CancellationToken cancellationToken = default)
    {
        if (notification is INotification n) Published.Add(n);
        return Task.CompletedTask;
    }

    public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        if (notification is INotification n) Published.Add(n);
        return Task.CompletedTask;
    }
}
