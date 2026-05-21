function ChevronLeft() {
  return (
    <svg
      width="12"
      height="12"
      viewBox="0 0 20 20"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.6"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <path d="M12 5l-5 5 5 5" />
    </svg>
  );
}

function ChevronRight() {
  return (
    <svg
      width="12"
      height="12"
      viewBox="0 0 20 20"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.6"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <path d="M8 5l5 5-5 5" />
    </svg>
  );
}

const PAGES = ["1", "2", "3", "4", "5"] as const;

export function OrdersInboxPagination({
  showing,
  total,
}: {
  showing: string;
  total: number;
}) {
  return (
    <div className="flex items-center justify-between border-t border-black/[0.06] px-7 py-3">
      <span className="text-[11px] text-foreground/50">
        Showing {showing} of {total}
      </span>
      <div className="flex items-center gap-1">
        <button
          type="button"
          aria-label="Previous page"
          className="inline-flex size-7 items-center justify-center rounded-md border border-black/[0.08] text-foreground/50 hover:bg-black/[0.03]"
        >
          <ChevronLeft />
        </button>
        {PAGES.map((n, i) => (
          <button
            key={n}
            type="button"
            aria-current={i === 0 ? "page" : undefined}
            aria-label={`Page ${n}`}
            className={`inline-flex min-w-[26px] items-center justify-center rounded-full px-2 py-0.5 text-[11px] font-semibold ${
              i === 0
                ? "bg-foreground text-white"
                : "border border-black/[0.08] text-foreground/60 hover:bg-black/[0.03]"
            }`}
          >
            {n}
          </button>
        ))}
        <button
          type="button"
          aria-label="Next page"
          className="inline-flex size-7 items-center justify-center rounded-md border border-black/[0.08] text-foreground/50 hover:bg-black/[0.03]"
        >
          <ChevronRight />
        </button>
      </div>
    </div>
  );
}
