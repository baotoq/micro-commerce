// web/src/lib/seller/payouts/data.ts
import type { LedgerEntry, PayoutSummary } from "@/lib/seller/payouts/types";

const LEDGER_ENTRIES: LedgerEntry[] = [
  {
    date: "Apr 8",
    label: "Payout · weekly",
    subject: "Sent · ending 4421",
    amount: 1284.62,
    type: "payout",
  },
  {
    date: "Apr 7",
    label: "Order #1042 · Sasha L.",
    subject: "Net of $3.44 fee",
    amount: 82.56,
    type: "sale",
  },
  {
    date: "Apr 7",
    label: "Order #1041 · Devon T.",
    subject: "Net of $2.56 fee",
    amount: 61.44,
    type: "sale",
  },
  {
    date: "Apr 6",
    label: "Shipping label · USPS",
    subject: "#1041 · 1lb 4oz",
    amount: -6.52,
    type: "fee",
  },
  {
    date: "Apr 5",
    label: "Order #1040 · Ari K.",
    subject: "Net of $1.92 fee",
    amount: 46.08,
    type: "sale",
  },
  {
    date: "Apr 1",
    label: "Payout · weekly",
    subject: "Sent · ending 4421",
    amount: 624.18,
    type: "payout",
  },
];
export function getLedgerEntries(): LedgerEntry[] {
  return LEDGER_ENTRIES;
}

const PAYOUT_SUMMARY: PayoutSummary = {
  lastPayout: 1284.62,
  lastPayoutSentLabel: "Sent · arriving Wed",
  available: 184.08,
  availableSendsOn: "Tue, Apr 15",
  availableFromOrders: 3,
  lifetime: 2148.36,
  lifetimeOrders: 23,
  lifetimeRange: "Mar 12 → Apr 11",
};
export function getPayoutSummary(): PayoutSummary {
  return PAYOUT_SUMMARY;
}
