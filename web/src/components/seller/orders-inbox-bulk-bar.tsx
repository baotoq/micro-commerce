export function OrdersInboxBulkBar({
  selectedCount,
  total,
}: {
  selectedCount: number;
  total: string;
}) {
  return (
    <div className="flex items-center justify-between border-b border-black/[0.06] bg-primary/[0.06] px-7 py-2.5">
      <div className="flex items-center gap-3">
        <span className="text-[13px] font-semibold text-primary">
          {selectedCount} orders selected
        </span>
        <span className="text-[12px] text-muted-foreground">
          — {total} total
        </span>
      </div>
      <div className="flex gap-2">
        <button
          type="button"
          className="rounded-lg border border-black/[0.12] px-3 py-1.5 text-[12px] font-medium text-foreground hover:bg-black/[0.03]"
        >
          Print labels
        </button>
        <button
          type="button"
          className="rounded-lg border border-black/[0.12] px-3 py-1.5 text-[12px] font-medium text-foreground hover:bg-black/[0.03]"
        >
          Mark packed
        </button>
        <button
          type="button"
          className="rounded-lg border border-black/[0.12] px-3 py-1.5 text-[12px] font-medium text-foreground hover:bg-black/[0.03]"
        >
          Bulk message
        </button>
        <button
          type="button"
          className="rounded-lg px-3 py-1.5 text-[12px] font-medium text-muted-foreground hover:bg-black/[0.03]"
        >
          Cancel
        </button>
      </div>
    </div>
  );
}
