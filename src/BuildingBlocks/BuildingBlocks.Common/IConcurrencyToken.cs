namespace MicroCommerce.BuildingBlocks.Common;

public interface IConcurrencyToken
{
    int Version { get; set; }
}
