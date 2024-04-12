using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Common;

public abstract class EntityBase : DateEntity
{
    [MaxLength(Constant.KeyLength)]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string Id { get; set; } = null!;
    
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