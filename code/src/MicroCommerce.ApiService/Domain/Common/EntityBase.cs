using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using MicroCommerce.ApiService.Domain.Events;

namespace MicroCommerce.ApiService.Domain.Common;

public abstract class EntityBase : DateEntity
{
    [Key]
    [MaxLength(Constant.KeyLength)]
    public Guid Id { get; init; }

    protected EntityBase()
    {
        Id = Guid.CreateVersion7(CreatedAt);
    }

    private readonly ConcurrentQueue<IDomainEvent> _eventStore = new();

    [JsonIgnore]
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
