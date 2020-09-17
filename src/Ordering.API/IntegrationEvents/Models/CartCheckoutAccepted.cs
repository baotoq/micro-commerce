namespace Ordering.API.IntegrationEvents.Models
{
    public class CartCheckoutAccepted : BaseMessage
    {
        public int CartId { get; set; }
    }
}
