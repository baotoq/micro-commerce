using MicroCommerce.Catalog.Application.Orders.Dtos;

namespace MicroCommerce.Catalog.Application.Analytics.Dtos;

public record KpiPointDto(string Label, decimal Value, decimal Change, bool IsUp);

public record RevenuePointDto(string Date, decimal Revenue, int Orders);

public record SourceBreakdownDto(string Source, decimal Pct, decimal Revenue);

public record FunnelStageDto(string Label, int Count, decimal Rate);

public record TopProductDto(string Sku, string Name, int Units, decimal Revenue);

public record AnalyticsOverviewDto(
    IReadOnlyList<KpiPointDto> KpiCards,
    IReadOnlyList<RevenuePointDto> RevenueSeries,
    IReadOnlyList<SourceBreakdownDto> Sources,
    IReadOnlyList<TopProductDto> TopProducts,
    IReadOnlyList<FunnelStageDto> Funnel);

public record DashboardTodayItemDto(string Label, int? Count);

public record DashboardSummaryDto(
    string DateLabel,
    int NewOrders,
    int OrdersToShip,
    decimal Revenue,
    int RecentReviews,
    IReadOnlyList<DashboardTodayItemDto> TodayItems,
    IReadOnlyList<OrderInboxRowDto> RecentOrders);
