import type { MarketingDraft } from "@/lib/seller/marketing/types";

export function EmailComposerContent({ draft }: { draft: MarketingDraft }) {
  return (
    <>
      <p className="mb-2 mt-6 text-[10px] font-semibold uppercase tracking-widest text-foreground/50">
        Step 2 of 3 · Content
      </p>
      <div className="rounded-xl border border-black/[0.06] bg-white p-[18px]">
        <div className="text-[11px] text-foreground/50">Template</div>
        <div className="mt-2 flex flex-wrap gap-2">
          {draft.templates.map((t) => (
            <span
              key={t.label}
              className="rounded-full px-3 py-1 text-[11.5px] font-medium"
              style={{
                background: t.on ? "var(--foreground)" : "rgba(0,0,0,0.05)",
                color: t.on ? "white" : "var(--foreground)",
              }}
            >
              {t.label}
            </span>
          ))}
        </div>

        <div className="mt-[18px] mb-1.5 text-[11px] text-foreground/50">
          Subject
        </div>
        <div
          className="flex items-center rounded-[10px] px-3.5"
          style={{
            height: 44,
            border: "1.5px solid var(--foreground)",
          }}
        >
          <span className="text-[14px] font-semibold text-foreground">
            {draft.subject}
          </span>
          <span
            className="ml-0.5 inline-block shrink-0"
            style={{
              width: 1.5,
              height: 18,
              background: "var(--foreground)",
              animation: "blink 1s steps(1) infinite",
            }}
          />
        </div>
        <div className="mt-1.5 flex justify-between text-[11px] text-foreground/50">
          <span>
            Open-rate forecast: <b className="text-foreground/80">32%</b> (above
            your avg)
          </span>
          <span>
            {draft.subjectCharCount} / {draft.subjectMaxChars}
          </span>
        </div>

        <div className="mt-[18px] mb-1.5 text-[11px] text-foreground/50">
          Preview text
        </div>
        <div
          className="flex items-center rounded-lg px-3"
          style={{
            height: 38,
            border: "1px solid rgba(0,0,0,0.1)",
          }}
        >
          <span className="text-[13px] text-foreground">
            {draft.previewText}
          </span>
        </div>

        <div className="mt-[18px] mb-2 text-[11px] text-foreground/50">
          Featured product
        </div>
        <div
          className="flex items-center gap-3 rounded-[10px] p-3"
          style={{ border: "1px solid rgba(0,0,0,0.1)" }}
        >
          <div
            className="shrink-0 rounded-lg"
            style={{
              width: 56,
              height: 56,
              background: "#d6c2a8",
              borderRadius: 8,
            }}
          />
          <div className="flex-1 min-w-0">
            <div className="text-[13px] font-semibold text-foreground">
              {draft.productName}
            </div>
            <div className="text-[11px] text-foreground/50">
              {draft.productInventoryLabel}
            </div>
          </div>
          <button
            type="button"
            className="rounded-lg px-3 py-1.5 text-[12px] font-medium text-foreground hover:bg-black/[0.04]"
          >
            Change
          </button>
        </div>
      </div>
    </>
  );
}
