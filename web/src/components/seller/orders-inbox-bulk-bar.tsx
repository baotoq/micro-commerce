export function OrdersInboxBulkBar({
  selectedCount,
  total,
}: {
  selectedCount: number;
  total: string;
}) {
  return (
    <div className="flex items-center justify-between border-b border-black/[0.06] bg-[rgba(0,102,204,0.06)] px-7 py-2.5">
      <div className="flex items-center gap-3">
        <span className="text-[13px] font-semibold text-[#0066cc]">
          {selectedCount} orders selected
        </span>
        <span className="text-[12px] text-[#1d1d1f]/50">— {total} total</span>
      </div>
      <div className="flex gap-2">
        <button
          type="button"
          className="rounded-lg border border-black/[0.12] px-3 py-1.5 text-[12px] font-medium text-[#1d1d1f] hover:bg-black/[0.03]"
        >
          Print labels
        </button>
        <button
          type="button"
          className="rounded-lg border border-black/[0.12] px-3 py-1.5 text-[12px] font-medium text-[#1d1d1f] hover:bg-black/[0.03]"
        >
          Mark packed
        </button>
        <button
          type="button"
          className="rounded-lg border border-black/[0.12] px-3 py-1.5 text-[12px] font-medium text-[#1d1d1f] hover:bg-black/[0.03]"
        >
          Bulk message
        </button>
        <button
          type="button"
          className="rounded-lg px-3 py-1.5 text-[12px] font-medium text-[#1d1d1f]/50 hover:bg-black/[0.03]"
        >
          Cancel
        </button>
      </div>
    </div>
  );
}
