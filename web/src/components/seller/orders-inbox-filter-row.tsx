function ChevronDown() {
  return (
    <svg
      width="10"
      height="10"
      viewBox="0 0 20 20"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <path d="M5 8l5 5 5-5" />
    </svg>
  );
}

function SearchIcon() {
  return (
    <svg
      width="13"
      height="13"
      viewBox="0 0 20 20"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.8"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
      className="shrink-0 text-[#1d1d1f]/40"
    >
      <circle cx="9" cy="9" r="5.5" />
      <path d="M13.5 13.5L17 17" />
    </svg>
  );
}

function FilterIcon() {
  return (
    <svg
      width="11"
      height="11"
      viewBox="0 0 20 20"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.8"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <path d="M3 5h14M6 10h8M9 15h2" />
    </svg>
  );
}

const FILTER_CHIPS = ["Status", "Ship method", "Date · last 30d"] as const;

export function OrdersInboxFilterRow() {
  return (
    <div className="flex items-center gap-2.5 border-b border-black/[0.06] px-7 py-3.5">
      <div className="flex h-8 max-w-[360px] flex-1 items-center gap-2 rounded-full bg-black/[0.05] px-3">
        <SearchIcon />
        <span className="text-[12px] text-[#1d1d1f]/40">
          Search by order, customer, SKU…
        </span>
      </div>
      {FILTER_CHIPS.map((label) => (
        <button
          key={label}
          type="button"
          className="flex h-[30px] items-center gap-1 rounded-full border border-black/[0.10] px-3 text-[11.5px] font-medium text-[#1d1d1f]/70 hover:bg-black/[0.03]"
        >
          {label}
          <ChevronDown />
        </button>
      ))}
      <span className="flex-1" />
      <button
        type="button"
        className="flex items-center gap-1.5 rounded-lg px-3 py-1.5 text-[12px] font-medium text-[#1d1d1f]/60 hover:bg-black/[0.03]"
      >
        <FilterIcon />
        More filters
      </button>
    </div>
  );
}
