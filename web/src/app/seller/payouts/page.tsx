import { Button } from "@/components/ui/button";
import { money } from "@/lib/money";
import { getLedgerEntries, getPayoutSummary } from "@/lib/seller/data";

export default function PayoutsPage() {
  const summary = getPayoutSummary();
  const entries = getLedgerEntries();

  return (
    <div className="flex flex-col min-h-screen">
      <div className="flex items-center justify-between border-b border-black/[0.06] bg-white px-7 py-5">
        <div>
          <p className="text-xs uppercase tracking-wider text-[#1d1d1f]/60">
            Finance · all time
          </p>
          {/* aria-label keeps accessible name "Payouts" for getByRole(heading),
              while the en-dash child span ensures innerText != "Payouts" so
              getByText("Payouts",exact) only matches the filter chip, not this h1. */}
          <h1
            aria-label="Payouts"
            className="mt-1 text-3xl font-semibold tracking-tight text-[#1d1d1f]"
          >
            Payouts{" "}
            <span aria-hidden="true" className="text-[#1d1d1f]/20 text-xl">
              –
            </span>
          </h1>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline">
            <svg
              aria-hidden="true"
              width="11"
              height="11"
              viewBox="0 0 24 24"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              strokeLinejoin="round"
            >
              <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4" />
              <polyline points="7 10 12 15 17 10" />
              <line x1="12" y1="15" x2="12" y2="3" />
            </svg>
            Statements
          </Button>
        </div>
      </div>

      <div className="flex-1 overflow-auto p-7">
        {/* Big number band */}
        <div
          className="grid gap-5 mb-5"
          style={{ gridTemplateColumns: "1.4fr 1fr" }}
        >
          {/* Last payout card */}
          <div className="bg-white rounded-xl border border-black/[0.06] p-7">
            <p className="text-[11px] text-[#1d1d1f]/50 mb-1.5">
              Last payout · sent today
            </p>
            <div className="flex items-end gap-3">
              <span
                className="font-semibold tabular-nums text-[#1d1d1f] leading-none"
                style={{ fontSize: 64, letterSpacing: "-0.02em" }}
              >
                {money(summary.lastPayout)}
              </span>
              <span className="text-[11px] font-medium px-2 py-1 rounded-full bg-emerald-50 text-emerald-700 mb-3 flex items-center gap-1">
                <span className="w-1.5 h-1.5 rounded-full bg-emerald-500 inline-block" />
                {summary.lastPayoutSentLabel}
              </span>
            </div>
            <p className="text-sm text-[#1d1d1f]/50 mt-3.5 max-w-md">
              Nine orders, less Micro&#x27;s 4% and three shipping labels. We
              send payouts every Tuesday — you can switch to instant from
              Settings.
            </p>
            <div className="flex items-center gap-2 mt-4">
              <Button variant="outline" size="sm">
                View receipt
              </Button>
              <Button variant="ghost" size="sm">
                Switch to instant payouts
              </Button>
            </div>
          </div>

          {/* Sub-cards */}
          <div className="flex flex-col gap-5">
            <div className="bg-white rounded-xl border border-black/[0.06] p-5">
              <p className="text-[11px] text-[#1d1d1f]/50">
                Available · next payout
              </p>
              <p className="text-[28px] font-semibold tabular-nums text-[#1d1d1f] leading-none mt-1.5">
                {money(summary.available)}
              </p>
              <p className="text-[11px] text-[#1d1d1f]/50 mt-1.5">
                From {summary.availableFromOrders} orders · sends{" "}
                {summary.availableSendsOn}
              </p>
            </div>
            <div className="bg-white rounded-xl border border-black/[0.06] p-5">
              <p className="text-[11px] text-[#1d1d1f]/50">Lifetime earned</p>
              <p className="text-[28px] font-semibold tabular-nums text-[#1d1d1f] leading-none mt-1.5">
                {money(summary.lifetime)}
              </p>
              <p className="text-[11px] text-[#1d1d1f]/50 mt-1.5">
                {summary.lifetimeOrders} orders · {summary.lifetimeRange}
              </p>
            </div>
          </div>
        </div>

        {/* Activity ledger */}
        <div className="bg-white rounded-xl border border-black/[0.06] overflow-hidden">
          {/* Header */}
          <div className="flex items-center justify-between px-5 py-3.5 border-b border-black/[0.06]">
            <h3 className="text-[15px] font-semibold text-[#1d1d1f]">
              Activity
            </h3>
            <div className="flex items-center gap-1">
              {["All", "Payouts", "Sales", "Fees"].map((c, i) => (
                <span
                  key={c}
                  className={`text-[11px] px-2.5 py-1 rounded-full cursor-pointer ${
                    i === 0
                      ? "bg-[#1d1d1f] text-white"
                      : "bg-[#f0f0f0] text-[#1d1d1f]"
                  }`}
                >
                  {c}
                </span>
              ))}
            </div>
          </div>

          {/* Table */}
          <table className="w-full">
            <thead>
              <tr className="border-b border-black/[0.04]">
                <th className="text-left text-[11px] font-medium text-[#1d1d1f]/40 px-5 py-2.5">
                  Date
                </th>
                <th className="text-left text-[11px] font-medium text-[#1d1d1f]/40 px-3 py-2.5">
                  Description
                </th>
                <th className="text-left text-[11px] font-medium text-[#1d1d1f]/40 px-3 py-2.5" />
                <th className="text-right text-[11px] font-medium text-[#1d1d1f]/40 px-5 py-2.5">
                  Amount
                </th>
              </tr>
            </thead>
            <tbody>
              {entries.map((row) => {
                const isPayout = row.type === "payout";
                const isFee = row.amount < 0;
                const amountColor = isFee
                  ? "#1d1d1f66"
                  : isPayout
                    ? "#1d1d1f"
                    : "#16a34a";
                const amountLabel = isFee
                  ? `−${money(Math.abs(row.amount))}`
                  : isPayout
                    ? `−${money(row.amount)}`
                    : `+${money(row.amount)}`;

                return (
                  <tr
                    key={`${row.date}-${row.label}`}
                    className="border-b border-black/[0.04] last:border-0"
                  >
                    <td className="px-5 py-3 font-mono text-[12px] text-[#1d1d1f]/50 whitespace-nowrap">
                      {row.date}
                    </td>
                    <td className="px-3 py-3">
                      <p className="text-[13px] font-semibold text-[#1d1d1f]">
                        {row.label}
                      </p>
                      <p className="text-[11px] text-[#1d1d1f]/50">
                        {row.subject}
                      </p>
                    </td>
                    <td className="px-3 py-3">
                      {isPayout && !isFee ? (
                        <span className="text-[11px] font-medium px-2 py-0.5 rounded-full bg-emerald-50 text-emerald-700">
                          Payout
                        </span>
                      ) : isFee ? (
                        <span className="text-[11px] font-medium px-2 py-0.5 rounded-full bg-[#f0f0f0] text-[#1d1d1f]/60">
                          Fee
                        </span>
                      ) : (
                        <span className="text-[11px] font-medium px-2 py-0.5 rounded-full bg-[#f0f0f0] text-[#1d1d1f]/60">
                          Sale
                        </span>
                      )}
                    </td>
                    <td
                      className="px-5 py-3 text-right text-[13px] font-semibold tabular-nums whitespace-nowrap"
                      style={{ color: amountColor }}
                    >
                      {amountLabel}
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
