// web/src/lib/seller/payouts/types.ts
export type LedgerEntry = {
  date: string;
  label: string;
  subject: string;
  amount: number;
  type: "payout" | "sale" | "fee";
};

export type PayoutSummary = {
  lastPayout: number;
  lastPayoutSentLabel: string;
  available: number;
  availableSendsOn: string;
  availableFromOrders: number;
  lifetime: number;
  lifetimeOrders: number;
  lifetimeRange: string;
};
