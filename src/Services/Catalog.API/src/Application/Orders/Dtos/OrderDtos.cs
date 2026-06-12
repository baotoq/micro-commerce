namespace MicroCommerce.Catalog.Application.Orders.Dtos;

public record OrderInboxRowDto(
    int Number,
    DateTimeOffset PlacedAt,
    string CustomerName,
    string CityState,
    string ItemsSummary,
    int Qty,
    decimal Total,
    string ShippingMethod,
    string Status,
    bool Starred);

public record OrderCountsDto(
    int All,
    int NeedsAction,
    int New,
    int Packed,
    int Shipped,
    int Delivered,
    int RefundOrCancel,
    decimal LifetimeRevenue);

public record OrderLineDto(
    int Idx,
    int Of,
    string Status,
    string Sku,
    string ProductName,
    string Tone,
    int Qty,
    decimal UnitPrice,
    string? Tracking,
    string? RestockNote);

public record RefundDraftItemDto(
    string Sku,
    int Qty,
    decimal Amount,
    bool Selected,
    decimal? PartialAmount);

public record RefundDraftDto(
    decimal Refundable,
    int ItemsCount,
    IReadOnlyList<RefundDraftItemDto> Items,
    string Reason,
    string RestockChoice,
    decimal Total,
    string LastFour);

public record OrderCustomerDto(
    string Name,
    string Email,
    string ShipLine1,
    string ShipLine2,
    bool BillSameAsShip,
    int LifetimeOrderCount,
    decimal LifetimeSpend,
    IReadOnlyList<string> Tags);

public record OrderSummaryDto(
    decimal Subtotal,
    int ItemsCount,
    decimal Shipping,
    decimal Tax,
    decimal Paid,
    decimal FeePct,
    decimal Fee,
    string? LabelCarrier,
    decimal LabelCost,
    decimal Net,
    string? DiscountCode = null,
    decimal DiscountAmount = 0m);

public record OrderTimelineDto(
    string Icon,
    string Title,
    string Sub,
    DateTimeOffset OccurredAt,
    string? Tone,
    bool Highlight);

public record OrderDetailDto(
    int Number,
    string Status,
    DateTimeOffset PlacedAt,
    IReadOnlyList<OrderLineDto> Lines,
    RefundDraftDto? Refund,
    OrderCustomerDto Customer,
    OrderSummaryDto Summary,
    string InternalNote,
    IReadOnlyList<OrderTimelineDto> Timeline,
    string? OutstandingProductName);
