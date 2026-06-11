namespace MicroCommerce.Catalog.Domain.Analytics;

/// <summary>
/// Synthetic seeded traffic-source breakdown row. Plain data container with no behavior —
/// analytics has no behavioral domain layer; these rows hold static seeded values.
/// </summary>
public class AnalyticsSource
{
    public int Id { get; private set; }
    public string Source { get; private set; } = string.Empty;
    public decimal Pct { get; private set; }
    public decimal Revenue { get; private set; }

    private AnalyticsSource() { }

    public AnalyticsSource(int id, string source, decimal pct, decimal revenue)
    {
        Id = id;
        Source = source;
        Pct = pct;
        Revenue = revenue;
    }
}
