namespace MicroCommerce.Catalog.Application.Analytics;

internal static class AnalyticsMapping
{
    /// <summary>
    /// Fixed dashboard date label derived from the demo anchor (2026-04-08 = a Tuesday).
    /// Hardcoded because it derives from the fixed DemoClock anchor, not from runtime "now".
    /// </summary>
    public const string DateLabel = "Tuesday · April 8";

    /// <summary>
    /// Synthetic constant: no reviews domain exists. The dashboard shows a fixed count.
    /// </summary>
    public const int RecentReviews = 3;
}
