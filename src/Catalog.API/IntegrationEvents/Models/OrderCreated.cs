namespace Catalog.API.IntegrationEvents.Models
{
    public class OrderCreated : BaseMessage
    {
        public long OrderId { get; set; }
    }
}
