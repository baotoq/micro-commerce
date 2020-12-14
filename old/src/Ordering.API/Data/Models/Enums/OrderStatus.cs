namespace Ordering.API.Data.Models.Enums
{
    public enum OrderStatus
    {
        New,
        PaymentReceived,
        Invoiced,
        Shipping,
        Completed,
        Canceled,
        Closed
    }
}
