import { Switch } from "@/components/ui/switch";
import type { MarketingDraft } from "@/lib/seller/marketing/types";

export function EmailComposerSchedule({ draft }: { draft: MarketingDraft }) {
  return (
    <>
      <p className="mb-2 mt-6 text-[10px] font-semibold uppercase tracking-widest text-foreground/50">
        Step 3 of 3 · Schedule
      </p>
      <div className="rounded-xl border border-black/[0.06] bg-white p-[18px]">
        <div className="grid grid-cols-2 gap-3">
          <div>
            <div className="mb-1.5 text-[11px] text-foreground/50">Send</div>
            <div className="flex flex-col gap-2">
              {draft.schedule.map((opt) => (
                <div
                  key={opt.label}
                  className="flex items-center gap-2 rounded-lg p-2"
                  style={{
                    border: opt.on
                      ? "1.5px solid var(--foreground)"
                      : "1px solid rgba(0,0,0,0.1)",
                  }}
                >
                  <span
                    className="flex shrink-0 items-center justify-center rounded-full"
                    style={{
                      width: 14,
                      height: 14,
                      border: "1.5px solid var(--foreground)",
                    }}
                  >
                    {opt.on && (
                      <span
                        className="rounded-full bg-foreground"
                        style={{ width: 7, height: 7 }}
                      />
                    )}
                  </span>
                  <div>
                    <div className="text-[12px] font-medium text-foreground">
                      {opt.label}
                    </div>
                    <div className="text-[11px] text-foreground/50">
                      {opt.sub}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>

          <div>
            <div className="mb-1.5 text-[11px] text-foreground/50">
              Follow-ups
            </div>
            <div className="flex flex-col gap-2">
              {draft.followups.map((f) => (
                <div
                  key={f.label}
                  className="flex items-center gap-2 rounded-lg p-2.5"
                  style={{ border: "1px solid rgba(0,0,0,0.1)" }}
                >
                  <Switch
                    defaultChecked={f.on}
                    size="sm"
                    aria-label={f.label}
                  />
                  <span className="flex-1 text-[12px] text-foreground">
                    {f.label}
                  </span>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </>
  );
}
