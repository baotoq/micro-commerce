import type { MarketingDraft } from "@/lib/seller/types";

export function EmailPreview({ draft }: { draft: MarketingDraft }) {
  return (
    <>
      <div className="mb-[14px] flex items-center justify-between">
        <span className="text-[13px] font-semibold tracking-tight text-foreground">
          Preview
        </span>
        <div
          className="flex rounded-full p-0.5"
          style={{ background: "white" }}
        >
          <span
            className="flex size-7 items-center justify-center rounded-full"
            style={{ background: "var(--foreground)", color: "white" }}
            role="img"
            aria-label="Light mode"
          >
            <svg
              aria-hidden="true"
              width="12"
              height="12"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <circle cx="12" cy="12" r="4" />
              <line x1="12" y1="2" x2="12" y2="6" />
              <line x1="12" y1="18" x2="12" y2="22" />
              <line x1="4.93" y1="4.93" x2="7.76" y2="7.76" />
              <line x1="16.24" y1="16.24" x2="19.07" y2="19.07" />
              <line x1="2" y1="12" x2="6" y2="12" />
              <line x1="18" y1="12" x2="22" y2="12" />
              <line x1="4.93" y1="19.07" x2="7.76" y2="16.24" />
              <line x1="16.24" y1="7.76" x2="19.07" y2="4.93" />
            </svg>
          </span>
          <span
            className="flex size-7 items-center justify-center rounded-full"
            style={{ background: "transparent", color: "rgba(29,29,31,0.4)" }}
            role="img"
            aria-label="Dark mode"
          >
            <svg
              aria-hidden="true"
              width="12"
              height="12"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z" />
            </svg>
          </span>
        </div>
      </div>

      {/* Email mock card */}
      <div
        className="overflow-hidden rounded-xl border border-black/[0.06] bg-white"
        style={{ padding: 0 }}
      >
        {/* Email header */}
        <div
          className="border-b border-black/[0.06] bg-white"
          style={{ padding: "14px 18px" }}
        >
          <div className="flex items-center gap-2">
            <div
              className="flex shrink-0 items-center justify-center rounded-full bg-foreground text-[11px] font-semibold text-white"
              style={{ width: 28, height: 28 }}
            >
              {draft.senderInitial}
            </div>
            <div className="flex-1 min-w-0">
              <div className="text-[12.5px] font-semibold text-foreground">
                {draft.senderName}
              </div>
              <div className="text-[11px] text-muted-foreground">
                to you · Tue 6:00 PM
              </div>
            </div>
          </div>
          <div
            className="mt-2.5 font-semibold text-foreground"
            style={{ fontSize: 15, lineHeight: 1.3 }}
          >
            {draft.subject}
          </div>
          <div className="mt-0.5 text-[11px] text-muted-foreground">
            {draft.previewText}
          </div>
        </div>

        {/* Hero image */}
        <div style={{ height: 220, background: "#d6c2a8" }} />

        {/* Body */}
        <div style={{ padding: "20px 22px" }}>
          <div
            className="font-semibold text-foreground"
            style={{ fontSize: 22, lineHeight: 1.1, letterSpacing: "-0.02em" }}
          >
            {draft.greeting}
          </div>
          {draft.body.map((para) => (
            <p
              key={para}
              className="text-foreground"
              style={{ marginTop: 10, fontSize: 13.5, lineHeight: 1.55 }}
            >
              {para}
            </p>
          ))}
          <button
            type="button"
            className="mt-[18px] rounded-pill bg-foreground text-[13px] font-medium text-white"
            style={{ height: 44, padding: "0 26px" }}
          >
            {draft.ctaLabel}
          </button>
          <div className="mt-4 text-[11px] text-muted-foreground">
            {draft.signoff}
          </div>
        </div>

        {/* Footer */}
        <div
          className="border-t border-black/[0.06]"
          style={{
            padding: "12px 22px",
            background: "var(--canvas-parchment)",
          }}
        >
          <div className="text-[11px] text-muted-foreground">
            {draft.unsubscribeFooter} ·{" "}
            <button
              type="button"
              className="text-primary"
              style={{
                background: "none",
                border: "none",
                padding: 0,
                cursor: "pointer",
              }}
            >
              Unsubscribe
            </button>
          </div>
        </div>
      </div>
    </>
  );
}
