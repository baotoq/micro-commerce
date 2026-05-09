import type { MarketingDraft } from "@/lib/seller/types";

export function EmailComposerAudience({ draft }: { draft: MarketingDraft }) {
  return (
    <>
      <p className="mb-2 text-[10px] font-semibold uppercase tracking-widest text-[#1d1d1f]/50">
        Step 1 of 3 · Audience
      </p>
      <div className="rounded-xl border border-black/[0.06] bg-white p-[18px]">
        <div className="mb-3 flex items-center justify-between">
          <span className="text-[13px] font-semibold tracking-tight text-[#1d1d1f]">
            Recipients
          </span>
          <span className="text-[11px] text-[#1d1d1f]/50">
            {draft.deliverable}
          </span>
        </div>
        <div className="flex flex-col gap-2">
          {draft.audiences.map((audience) => (
            <div
              key={audience.label}
              className="flex items-center gap-3 rounded-[10px] border p-3"
              style={{
                borderColor: "rgba(0,0,0,0.06)",
                background: audience.on ? "rgba(0,102,204,0.04)" : "white",
              }}
            >
              <Switch on={audience.on} />
              <div className="flex-1 min-w-0">
                <div className="text-[13px] font-semibold text-[#1d1d1f]">
                  {audience.label}
                </div>
                <div className="text-[11px] text-[#1d1d1f]/50">
                  {audience.sub}
                </div>
              </div>
              <span className="text-[13px] font-semibold tabular-nums text-[#1d1d1f]">
                {audience.count}
              </span>
            </div>
          ))}
        </div>
      </div>
    </>
  );
}

function Switch({ on }: { on: boolean }) {
  return (
    <span
      className="relative inline-flex shrink-0 items-center rounded-full transition-colors"
      style={{
        width: 28,
        height: 16,
        background: on ? "#1d1d1f" : "rgba(0,0,0,0.1)",
      }}
    >
      <span
        className="inline-block rounded-full bg-white shadow transition-transform"
        style={{
          width: 12,
          height: 12,
          transform: on ? "translateX(14px)" : "translateX(2px)",
        }}
      />
    </span>
  );
}
