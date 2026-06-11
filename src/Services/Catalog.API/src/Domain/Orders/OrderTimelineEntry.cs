namespace MicroCommerce.Catalog.Domain.Orders;

public class OrderTimelineEntry
{
    public string Icon { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Sub { get; private set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; private set; }
    public string? Tone { get; private set; }
    public bool Highlight { get; private set; }

    private OrderTimelineEntry() { }

    public OrderTimelineEntry(
        string icon,
        string title,
        string sub,
        DateTimeOffset occurredAt,
        string? tone,
        bool highlight)
    {
        if (string.IsNullOrWhiteSpace(icon))
            throw new ArgumentException("Icon required.", nameof(icon));
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title required.", nameof(title));

        Icon = icon;
        Title = title;
        Sub = sub;
        OccurredAt = occurredAt;
        Tone = tone;
        Highlight = highlight;
    }
}
