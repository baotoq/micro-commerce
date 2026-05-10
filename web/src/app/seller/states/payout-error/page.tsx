import { AlertTriangle, Info } from "lucide-react";
import Link from "next/link";
import { SellerTopbar } from "@/components/seller/seller-topbar";
import { Sparkline } from "@/components/seller/sparkline";
import { Button } from "@/components/ui/button";
import { money } from "@/lib/money";
import { BRAND, DATE_LABEL } from "@/lib/seller/data";

const HELD_AMOUNT = 1284.62;

const KPIS = [
  { label: "Revenue · 7 days", value: money(4280) },
  { label: "Orders · 7 days", value: "38" },
  { label: "Avg. order", value: money(112) },
] as const;

const SPARK_POINTS = [0.4, 0.5, 0.45, 0.6, 0.55, 0.7, 0.85];

export default function PayoutErrorPage() {
  return (
    <div className="flex flex-col min-h-screen">
      <SellerTopbar
        title={`Good morning, ${BRAND.owner}`}
        subtitle={DATE_LABEL}
      />

      {/* Error banner */}
      <div
        role="alert"
        style={{
          background: "#FBE9E5",
          borderBottom: "1px solid #E8B5AB",
          padding: "14px 28px",
        }}
      >
        <div className="flex items-start gap-3">
          <span
            className="flex items-center justify-center shrink-0"
            style={{
              width: 28,
              height: 28,
              borderRadius: 999,
              background: "var(--bad)",
              color: "white",
            }}
          >
            <AlertTriangle size={14} strokeWidth={2.2} />
          </span>
          <div className="flex-1">
            <h4
              className="font-semibold text-foreground text-[15px] leading-snug"
              style={{ marginBottom: 2 }}
            >
              We couldn&apos;t send your Tuesday payout · {money(HELD_AMOUNT)}{" "}
              held
            </h4>
            <p className="text-sm text-muted-foreground">
              Your bank rejected the transfer (account ending 4421). This
              sometimes happens after an address change. Update the account and
              we&apos;ll retry within an hour.
            </p>
          </div>
          <div className="flex items-center gap-2 shrink-0">
            <Button variant="outline" size="sm">
              View details
            </Button>
            {/* biome-ignore lint/a11y/useSemanticElements: link styled as button to navigate while preserving e2e button-role assertion */}
            <Link
              href="/seller/payouts"
              role="button"
              className="inline-flex items-center justify-center rounded-pill bg-primary px-2.5 text-[0.8rem] font-medium text-primary-foreground h-7"
            >
              Update bank →
            </Link>
          </div>
        </div>
      </div>

      <div className="flex-1 overflow-auto" style={{ padding: "24px 28px" }}>
        {/* Held payout card */}
        <div
          className="rounded-xl border bg-white"
          style={{
            padding: 24,
            marginBottom: 20,
            borderColor: "var(--bad)",
          }}
        >
          <div className="flex items-end justify-between">
            <div>
              <p
                className="text-[11px] font-semibold uppercase tracking-wide"
                style={{ color: "var(--bad)", marginBottom: 6 }}
              >
                ● Payout held
              </p>
              <p
                className="font-semibold tabular-nums text-foreground leading-none"
                style={{ fontSize: 36 }}
              >
                {money(HELD_AMOUNT)}
              </p>
              <p
                className="text-sm text-muted-foreground"
                style={{ marginTop: 6 }}
              >
                9 orders · weekly batch · would have arrived Wed
              </p>
            </div>
            <div className="flex items-center gap-2">
              <Button variant="outline">Switch payout method</Button>
              <Button>Retry payout</Button>
            </div>
          </div>

          <hr className="border-black/[0.06]" style={{ margin: "18px 0" }} />

          <div className="flex items-start gap-3">
            <Info size={14} className="text-muted-foreground mt-0.5 shrink-0" />
            <div>
              <h4
                className="font-semibold text-foreground"
                style={{ fontSize: 13, marginBottom: 2 }}
              >
                What&apos;s happening
              </h4>
              <p className="text-sm text-muted-foreground">
                Stripe returned{" "}
                <code className="font-mono text-foreground">
                  R03 · No account / unable to locate
                </code>
                . The funds are safe with us; nothing left your shop. Once you
                update the routing or account number, we&apos;ll auto-retry.
              </p>
            </div>
          </div>
        </div>

        {/* Dimmed KPI row */}
        <div
          className="grid gap-3"
          style={{ gridTemplateColumns: "repeat(3, 1fr)", opacity: 0.4 }}
        >
          {KPIS.map((kpi) => (
            <div
              key={kpi.label}
              className="rounded-xl border border-black/[0.06] bg-white"
              style={{ padding: 18 }}
            >
              <p className="text-[11px] text-muted-foreground">{kpi.label}</p>
              <p
                className="font-semibold tabular-nums text-foreground leading-none"
                style={{ fontSize: 28, marginTop: 6 }}
              >
                {kpi.value}
              </p>
              <div style={{ height: 32, marginTop: 8 }}>
                <Sparkline
                  points={SPARK_POINTS}
                  className="h-8 w-full text-muted-foreground"
                />
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
