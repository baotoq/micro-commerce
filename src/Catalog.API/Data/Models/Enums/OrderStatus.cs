namespace Catalog.API.Data.Models
{
    public enum OrderStatus
    {
        New = 1,

        PaymentReceived = 30,

        Invoiced = 40,

        Shipping = 50,

        Completed = 70,

        Canceled = 80,

        Closed = 100
    }
}
