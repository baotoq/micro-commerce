const DISCOUNT_OPTIONS = ["% off", "$ off", "Free shipping", "BOGO"] as const;

const WHO_OPTIONS = [
  {
    label: "Anyone with the code",
    sub: "Public · share on socials",
    on: false,
  },
  {
    label: "Followers only",
    sub: "Auto-applied · 184 buyers eligible",
    on: true,
  },
  { label: "Specific customers", sub: "Pick from your CRM", on: false },
] as const;

const LIMITS = [
  { label: "Min. order", value: "$40.00" },
  { label: "Per buyer", value: "1 use" },
  { label: "Total uses", value: "200" },
  { label: "Window", value: "Apr 22 → May 06", small: true },
] as const;

function CloseIcon() {
  return (
    <svg
      width="14"
      height="14"
      viewBox="0 0 20 20"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.6"
      strokeLinecap="round"
      aria-hidden="true"
    >
      <title>close</title>
      <path d="M4 4l12 12M16 4L4 16" />
    </svg>
  );
}

function RefreshIcon() {
  return (
    <svg
      width="11"
      height="11"
      viewBox="0 0 20 20"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.6"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <title>refresh</title>
      <path d="M4 4a8 8 0 0 1 12 0M16 16a8 8 0 0 1-12 0M17 6v4h-4M3 14v-4h4" />
    </svg>
  );
}

export function NewPromoDrawer() {
  return (
    <div
      className="absolute bottom-0 right-0 top-0 flex flex-col bg-white"
      style={{
        width: 460,
        borderLeft: "1px solid rgba(0,0,0,0.06)",
        boxShadow: "-12px 0 30px rgba(0,0,0,0.06)",
        zIndex: 30,
      }}
    >
      {/* Header */}
      <div
        className="flex items-center justify-between"
        style={{
          padding: "16px 22px",
          borderBottom: "1px solid rgba(0,0,0,0.06)",
        }}
      >
        <h2 className="text-[18px] font-semibold text-[#1d1d1f]">
          New promotion
        </h2>
        <button
          type="button"
          aria-label="Close"
          className="flex h-8 w-8 items-center justify-center rounded-full text-[#1d1d1f]/50 hover:bg-black/[0.04]"
        >
          <CloseIcon />
        </button>
      </div>

      {/* Body */}
      <div className="grow overflow-auto" style={{ padding: 22 }}>
        {/* CODE section */}
        <div
          className="text-[10px] font-semibold uppercase tracking-wider text-[#1d1d1f]/50"
          style={{ marginBottom: 8 }}
        >
          Code
        </div>
        <div
          data-testid="drawer-code-input"
          className="flex items-center gap-2"
          style={{
            height: 44,
            padding: "0 14px",
            border: "1.5px solid #1d1d1f",
            borderRadius: 10,
          }}
        >
          <span className="font-mono text-[15px] font-semibold text-[#1d1d1f]">
            STUDIO15
          </span>
          <span className="flex-1" />
          <button
            type="button"
            className="flex h-[26px] items-center gap-1 rounded-lg px-2 text-xs font-medium text-[#1d1d1f]/60 hover:bg-black/[0.04]"
          >
            <RefreshIcon />
            Generate
          </button>
        </div>
        <div className="mt-2 text-xs text-[#1d1d1f]/50">
          Buyers will type or click this at checkout
        </div>

        {/* DISCOUNT section */}
        <div
          className="text-[10px] font-semibold uppercase tracking-wider text-[#1d1d1f]/50"
          style={{ margin: "20px 0 8px" }}
        >
          Discount
        </div>
        <div
          className="flex w-fit gap-0 rounded-full p-[3px]"
          style={{ background: "#f5f5f7" }}
        >
          {DISCOUNT_OPTIONS.map((opt, i) => (
            <span
              key={opt}
              className="flex items-center justify-center rounded-full text-[12px]"
              style={{
                padding: "6px 14px",
                fontWeight: i === 0 ? 600 : 400,
                background: i === 0 ? "white" : "transparent",
                color: i === 0 ? "#1d1d1f" : "#1d1d1f99",
                boxShadow: i === 0 ? "0 1px 2px rgba(0,0,0,0.06)" : "none",
              }}
            >
              {opt}
            </span>
          ))}
        </div>
        <div className="mt-3.5 flex items-center gap-2">
          <div
            className="flex items-center gap-1.5"
            style={{
              height: 56,
              padding: "0 18px",
              border: "1.5px solid #1d1d1f",
              borderRadius: 10,
            }}
          >
            <span
              className="font-semibold tabular-nums leading-none"
              style={{ fontSize: 32 }}
            >
              15
            </span>
            <span className="text-[15px] font-medium text-[#1d1d1f]/50">%</span>
          </div>
          <span className="text-sm text-[#1d1d1f]/50">
            off the entire order
          </span>
        </div>

        {/* WHO CAN USE IT section */}
        <div
          className="text-[10px] font-semibold uppercase tracking-wider text-[#1d1d1f]/50"
          style={{ margin: "22px 0 8px" }}
        >
          Who can use it
        </div>
        <div className="flex flex-col gap-2">
          {WHO_OPTIONS.map((opt) => (
            <div
              key={opt.label}
              className="flex items-start gap-2.5"
              style={{
                padding: 12,
                borderRadius: 10,
                border: opt.on
                  ? "1.5px solid #1d1d1f"
                  : "1px solid rgba(0,0,0,0.1)",
                background: opt.on ? "#f5f5f7" : "white",
              }}
            >
              <span
                className="mt-0.5 flex h-4 w-4 shrink-0 items-center justify-center rounded-full"
                style={{ border: "1.5px solid #1d1d1f" }}
              >
                {opt.on && (
                  <span
                    className="h-2 w-2 rounded-full"
                    style={{ background: "#1d1d1f" }}
                  />
                )}
              </span>
              <div>
                <div className="text-[13px] font-semibold text-[#1d1d1f]">
                  {opt.label}
                </div>
                <div className="text-xs text-[#1d1d1f]/50">{opt.sub}</div>
              </div>
            </div>
          ))}
        </div>

        {/* LIMITS section */}
        <div
          className="text-[10px] font-semibold uppercase tracking-wider text-[#1d1d1f]/50"
          style={{ margin: "22px 0 8px" }}
        >
          Limits
        </div>
        <div className="grid grid-cols-2 gap-2">
          {LIMITS.map((limit) => (
            <div
              key={limit.label}
              className="rounded-xl border border-black/[0.06] bg-white p-3"
            >
              <div className="text-xs text-[#1d1d1f]/50">{limit.label}</div>
              <div
                className="mt-0.5 font-semibold tabular-nums"
                style={{
                  fontSize: "small" in limit && limit.small ? 12.5 : undefined,
                }}
              >
                {limit.value}
              </div>
            </div>
          ))}
        </div>

        {/* Forecast callout */}
        <div
          className="mt-[18px] rounded-xl p-3.5"
          style={{ background: "#f5f5f7" }}
        >
          <div className="mb-1 flex items-center gap-2">
            <span className="inline-block h-1.5 w-1.5 rounded-full bg-emerald-500" />
            <span className="text-[12.5px] font-semibold text-[#1d1d1f]">
              Forecast
            </span>
          </div>
          <p className="text-xs text-[#1d1d1f]/50">
            At your follower count, expect{" "}
            <b className="text-[#1d1d1f]">~24 redemptions</b> driving{" "}
            <b className="text-[#1d1d1f]">$420–$640</b> in incremental revenue.
            Margin impact: <b className="text-[#1d1d1f]">-$72</b>.
          </p>
        </div>
      </div>

      {/* Footer */}
      <div
        className="flex items-center justify-between"
        style={{
          padding: "14px 22px",
          borderTop: "1px solid rgba(0,0,0,0.06)",
        }}
      >
        <button
          type="button"
          className="rounded-xl border border-black/[0.12] px-4 py-2 text-sm font-medium text-[#1d1d1f] hover:bg-black/[0.04]"
        >
          Save draft
        </button>
        <button
          type="button"
          className="rounded-xl bg-[#1d1d1f] px-4 py-2 text-sm font-semibold text-white hover:bg-[#1d1d1f]/90"
        >
          Activate · Tue 12:00 AM
        </button>
      </div>
    </div>
  );
}
