namespace MicroCommerce.Catalog.Domain.Analytics;

/// <summary>
/// Synthetic seeded conversion-funnel stage row. Plain data container with no behavior —
/// analytics has no behavioral domain layer; these rows hold static seeded values.
/// </summary>
public class AnalyticsFunnelStage
{
    public int Id { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public int Count { get; private set; }
    public decimal Rate { get; private set; }

    private AnalyticsFunnelStage() { }

    public AnalyticsFunnelStage(int id, string label, int count, decimal rate)
    {
        Id = id;
        Label = label;
        Count = count;
        Rate = rate;
    }
}
