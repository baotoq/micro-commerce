using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain;

public abstract class EntityBase
{
    private readonly ConcurrentQueue<IDomainEvent> _eventStore = new();

    [NotMapped]
    public IEnumerable<IDomainEvent> EventStore => _eventStore;

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _eventStore.Enqueue(domainEvent);
    }
    
    public void ClearEventStore()
    {
        _eventStore.Clear();
    }
}