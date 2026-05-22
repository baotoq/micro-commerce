import { Download } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { money } from "@/lib/money";
import { getLedgerEntries, getPayoutSummary } from "@/lib/seller/payouts/data";

export default function PayoutsPage() {
  const summary = getPayoutSummary();
  const entries = getLedgerEntries();

  return (
    <div className="flex flex-col min-h-screen">
      <div className="flex items-center justify-between border-b border-black/[0.06] bg-white px-7 py-5">
        <div>
          <p className="text-xs uppercase tracking-wider text-muted-foreground">
            Finance · all time
          </p>
          {/* aria-label keeps accessible name "Payouts" for getByRole(heading),
              while the en-dash child span ensures innerText != "Payouts" so
              getByText("Payouts",exact) only matches the filter chip, not this h1. */}
          <h1
            aria-label="Payouts"
            className="mt-1 text-3xl font-semibold tracking-tight text-foreground"
          >
            Payouts{" "}
            <span aria-hidden="true" className="text-muted-foreground text-xl">
              –
            </span>
          </h1>
        </div>
        <div className="flex items-center gap-2">
          <Button variant="outline">
            <Download aria-hidden="true" className="h-[11px] w-[11px]" />
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
            <p className="text-[11px] text-muted-foreground mb-1.5">
              Last payout · sent today
            </p>
            <div className="flex items-end gap-3">
              <span
                className="font-semibold tabular-nums text-foreground leading-none"
                style={{ fontSize: 64, letterSpacing: "-0.02em" }}
              >
                {money(summary.lastPayout)}
              </span>
              <span className="text-[11px] font-medium px-2 py-1 rounded-full bg-good/15 text-good mb-3 flex items-center gap-1">
                <span className="w-1.5 h-1.5 rounded-full bg-good inline-block" />
                {summary.lastPayoutSentLabel}
              </span>
            </div>
            <p className="text-sm text-muted-foreground mt-3.5 max-w-md">
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
              <p className="text-[11px] text-muted-foreground">
                Available · next payout
              </p>
              <p className="text-[28px] font-semibold tabular-nums text-foreground leading-none mt-1.5">
                {money(summary.available)}
              </p>
              <p className="text-[11px] text-muted-foreground mt-1.5">
                From {summary.availableFromOrders} orders · sends{" "}
                {summary.availableSendsOn}
              </p>
            </div>
            <div className="bg-white rounded-xl border border-black/[0.06] p-5">
              <p className="text-[11px] text-muted-foreground">
                Lifetime earned
              </p>
              <p className="text-[28px] font-semibold tabular-nums text-foreground leading-none mt-1.5">
                {money(summary.lifetime)}
              </p>
              <p className="text-[11px] text-muted-foreground mt-1.5">
                {summary.lifetimeOrders} orders · {summary.lifetimeRange}
              </p>
            </div>
          </div>
        </div>

        {/* Activity ledger */}
        <div className="bg-white rounded-xl border border-black/[0.06] overflow-hidden">
          {/* Header */}
          <div className="flex items-center justify-between px-5 py-3.5 border-b border-black/[0.06]">
            <h3 className="text-[15px] font-semibold text-foreground">
              Activity
            </h3>
            <div className="flex items-center gap-1">
              {["All", "Payouts", "Sales", "Fees"].map((c, i) => (
                <span
                  key={c}
                  className={`text-[11px] px-2.5 py-1 rounded-full cursor-pointer ${
                    i === 0
                      ? "bg-foreground text-white"
                      : "bg-muted text-foreground"
                  }`}
                >
                  {c}
                </span>
              ))}
            </div>
          </div>

          {/* Table */}
          <Table>
            <TableHeader>
              <TableRow className="text-[11px] font-medium text-muted-foreground">
                <TableHead className="px-5">Date</TableHead>
                <TableHead className="px-3">Description</TableHead>
                <TableHead className="px-3" />
                <TableHead className="px-5 text-right">Amount</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {entries.map((row) => {
                const isPayout = row.type === "payout";
                const isFee = row.amount < 0;
                const amountColor = isFee
                  ? "color-mix(in oklch, var(--foreground) 40%, transparent)"
                  : isPayout
                    ? "var(--foreground)"
                    : undefined;
                const amountLabel = isFee
                  ? `−${money(Math.abs(row.amount))}`
                  : isPayout
                    ? `−${money(row.amount)}`
                    : `+${money(row.amount)}`;

                return (
                  <TableRow key={`${row.date}-${row.label}`}>
                    <TableCell className="px-5 py-3 font-mono text-[12px] text-muted-foreground whitespace-nowrap">
                      {row.date}
                    </TableCell>
                    <TableCell className="px-3 py-3">
                      <p className="text-[13px] font-semibold text-foreground">
                        {row.label}
                      </p>
                      <p className="text-[11px] text-muted-foreground">
                        {row.subject}
                      </p>
                    </TableCell>
                    <TableCell className="px-3 py-3">
                      {isPayout && !isFee ? (
                        <span className="text-[11px] font-medium px-2 py-0.5 rounded-full bg-good/15 text-good">
                          Payout
                        </span>
                      ) : isFee ? (
                        <span className="text-[11px] font-medium px-2 py-0.5 rounded-full bg-muted text-muted-foreground">
                          Fee
                        </span>
                      ) : (
                        <span className="text-[11px] font-medium px-2 py-0.5 rounded-full bg-muted text-muted-foreground">
                          Sale
                        </span>
                      )}
                    </TableCell>
                    <TableCell
                      className={`px-5 py-3 text-right text-[13px] font-semibold tabular-nums whitespace-nowrap ${!isFee && !isPayout ? "text-good" : ""}`}
                      style={amountColor ? { color: amountColor } : undefined}
                    >
                      {amountLabel}
                    </TableCell>
                  </TableRow>
                );
              })}
            </TableBody>
          </Table>
        </div>
      </div>
    </div>
  );
}
