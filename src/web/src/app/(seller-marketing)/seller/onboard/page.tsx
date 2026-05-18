import Link from "next/link";
import { getSetupSteps } from "@/lib/seller/application/data";

const PAYOUT_OPTIONS = [
  { label: "Bank account", sub: "ACH · 1–2 days", selected: true },
  { label: "Debit card", sub: "Instant · 1.5%", selected: false },
  { label: "Add later", sub: "Launch in draft", selected: false },
];

export default function OnboardPage() {
  const steps = getSetupSteps();

  return (
    <div className="flex min-h-screen bg-canvas-parchment text-foreground">
      {/* Left rail */}
      <div
        className="flex flex-col bg-white border-r border-border"
        style={{
          width: 280,
          padding: "40px 28px",
          flexShrink: 0,
        }}
      >
        <div className="mb-7 flex items-center gap-2">
          <span className="font-semibold" style={{ fontSize: 18 }}>
            micro.
          </span>
          <span className="ml-1 text-[11px] text-foreground/60">
            Open a shop
          </span>
        </div>

        <p className="mb-1.5 text-[11px] text-foreground/60">Step 2 of 6</p>
        <div className="mb-5 h-1 overflow-hidden rounded-full bg-border">
          <div
            className="h-full rounded-full bg-foreground"
            style={{ width: "33%" }}
          />
        </div>

        <div className="flex flex-col gap-3">
          {steps.map((step, i) => {
            const isDone = step.status === "done";
            const isActive = step.status === "active";
            return (
              <div key={step.label} className="flex items-center gap-2">
                <span
                  className="flex shrink-0 items-center justify-center rounded-full text-[11px] font-semibold"
                  style={{
                    width: 22,
                    height: 22,
                    background: isDone
                      ? "var(--good)"
                      : isActive
                        ? "var(--foreground)"
                        : "var(--canvas-parchment)",
                    color: isDone || isActive ? "white" : "var(--foreground)",
                    opacity: isDone || isActive ? 1 : 0.6,
                    border:
                      isDone || isActive ? "none" : "1px solid var(--border)",
                  }}
                >
                  {isDone ? (
                    <svg
                      width="11"
                      height="11"
                      viewBox="0 0 11 11"
                      fill="none"
                      role="img"
                      aria-label="Done"
                    >
                      <polyline
                        points="2,5.5 4.5,8 9,3"
                        stroke="white"
                        strokeWidth="2"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                      />
                    </svg>
                  ) : (
                    i + 1
                  )}
                </span>
                <span
                  className="text-[13px]"
                  style={{
                    color: isActive ? "var(--foreground)" : "var(--foreground)",
                    opacity: isActive ? 1 : 0.6,
                    fontWeight: isActive ? 600 : 400,
                  }}
                >
                  {step.label}
                </span>
              </div>
            );
          })}
        </div>

        <div className="mt-8 rounded-lg p-3.5 bg-canvas-parchment">
          <p className="mb-1 text-[10px] font-semibold uppercase tracking-wider text-terra">
            Tip
          </p>
          <p className="text-[12px] text-foreground/60">
            Add payouts last if you'd like — you can launch in draft and finish
            this when an order comes in.
          </p>
        </div>
      </div>

      {/* Right form */}
      <div className="flex-1 overflow-y-auto" style={{ padding: "60px 56px" }}>
        <div style={{ maxWidth: 620 }}>
          <p className="mb-2 text-[11px] font-semibold uppercase tracking-wider text-foreground/60">
            Step 2 · Location &amp; payouts
          </p>
          <h1
            className="font-semibold leading-tight tracking-tight"
            style={{ fontSize: 40, letterSpacing: "-0.374px" }}
          >
            Where are you shipping from, and where should we send the money?
          </h1>

          {/* Studio location card */}
          <div className="mt-7 rounded-lg border border-black/[0.06] bg-white p-6">
            <h2 className="mb-1 text-[15px] font-semibold text-foreground">
              Studio location
            </h2>
            <p className="mb-3.5 text-[13px] text-foreground/60">
              Customers see only your city &amp; state.
            </p>
            <div
              className="grid gap-3"
              style={{ gridTemplateColumns: "2fr 1fr" }}
            >
              <div>
                <p className="mb-1 text-[11px] text-foreground/60">
                  Address line 1
                </p>
                <div className="flex h-9 items-center rounded-sm border border-black/[0.06] px-3 text-[13px]">
                  410 Linden St
                </div>
              </div>
              <div>
                <p className="mb-1 text-[11px] text-foreground/60">City</p>
                <div className="flex h-9 items-center rounded-sm border border-black/[0.06] px-3 text-[13px]">
                  Oakland
                </div>
              </div>
            </div>
          </div>

          {/* Payouts card */}
          <div className="mt-4 rounded-lg border border-black/[0.06] bg-white p-6">
            <div className="mb-3.5 flex items-center justify-between">
              <div>
                <h2 className="mb-0.5 text-[15px] font-semibold text-foreground">
                  Where to send your payouts
                </h2>
                <p className="text-[13px] text-foreground/60">
                  Pick one — you can add more later.
                </p>
              </div>
              <span className="rounded-full bg-canvas-parchment px-2.5 py-1 text-[11px] text-foreground/60">
                256-bit · Stripe
              </span>
            </div>
            <div
              className="grid gap-2"
              style={{ gridTemplateColumns: "1fr 1fr 1fr" }}
            >
              {PAYOUT_OPTIONS.map((opt) => (
                <div
                  key={opt.label}
                  className="rounded-lg p-3.5"
                  style={{
                    border: opt.selected
                      ? "1.5px solid var(--foreground)"
                      : "1px solid var(--border)",
                    background: opt.selected
                      ? "var(--canvas-parchment)"
                      : "white",
                  }}
                >
                  <div className="mb-1.5 flex items-center justify-between">
                    <span className="text-sm text-foreground/40">
                      {opt.label === "Bank account"
                        ? "🏦"
                        : opt.label === "Debit card"
                          ? "💳"
                          : "⏱"}
                    </span>
                    {opt.selected && (
                      <span
                        className="flex items-center justify-center rounded-full bg-foreground"
                        style={{ width: 18, height: 18 }}
                      >
                        <svg
                          width="10"
                          height="10"
                          viewBox="0 0 10 10"
                          fill="none"
                          role="img"
                          aria-label="Selected"
                        >
                          <polyline
                            points="2,5 4,7.5 8,2.5"
                            stroke="white"
                            strokeWidth="1.8"
                            strokeLinecap="round"
                            strokeLinejoin="round"
                          />
                        </svg>
                      </span>
                    )}
                  </div>
                  <p className="text-[13px] font-semibold text-foreground">
                    {opt.label}
                  </p>
                  <p className="text-[11px] text-foreground/60">{opt.sub}</p>
                </div>
              ))}
            </div>
          </div>

          {/* Action row */}
          <div className="mt-7 flex items-center justify-between">
            {/* biome-ignore lint/a11y/useSemanticElements: link styled as button to navigate while preserving e2e button-role assertion */}
            <Link
              href="/seller/apply"
              role="button"
              className="rounded-full px-5 py-2.5 text-sm font-medium text-foreground hover:bg-black/5"
            >
              ← Back
            </Link>
            <button
              type="button"
              className="rounded-full bg-primary px-6 py-2.5 text-sm font-medium text-white hover:bg-[#0055aa]"
              style={{ border: "none", cursor: "pointer" }}
            >
              Continue · Brand
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
