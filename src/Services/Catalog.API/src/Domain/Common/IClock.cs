namespace MicroCommerce.Catalog.Domain.Common;

public interface IClock
{
    DateTimeOffset Now { get; }
}
