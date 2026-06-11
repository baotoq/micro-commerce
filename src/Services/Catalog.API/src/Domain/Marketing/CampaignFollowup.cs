namespace MicroCommerce.Catalog.Domain.Marketing;

public class CampaignFollowup
{
    public bool Enabled { get; private set; }
    public FollowupKind Kind { get; private set; }
    public int DelayDays { get; private set; }
    public string Subject { get; private set; } = string.Empty;
    public string Preview { get; private set; } = string.Empty;

    private CampaignFollowup() { }

    public CampaignFollowup(
        bool enabled,
        FollowupKind kind,
        int delayDays,
        string subject,
        string preview)
    {
        if (delayDays < 0)
            throw new ArgumentOutOfRangeException(nameof(delayDays), "DelayDays cannot be negative.");
        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Followup subject required.", nameof(subject));
        if (string.IsNullOrWhiteSpace(preview))
            throw new ArgumentException("Followup preview required.", nameof(preview));

        Enabled = enabled;
        Kind = kind;
        DelayDays = delayDays;
        Subject = subject;
        Preview = preview;
    }
}
