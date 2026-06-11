using MicroCommerce.Catalog.Domain.Payouts;

namespace MicroCommerce.Catalog.Application.Payouts;

internal static class PayoutMapping
{
    public static string KindToString(LedgerEntryKind kind) => kind switch
    {
        LedgerEntryKind.Sale => "sale",
        LedgerEntryKind.Fee => "fee",
        LedgerEntryKind.Payout => "payout",
        LedgerEntryKind.Label => "label",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unknown ledger entry kind."),
    };
}
