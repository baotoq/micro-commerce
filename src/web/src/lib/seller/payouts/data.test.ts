// web/src/lib/seller/payouts/data.test.ts
import { describe, expect, it } from "vitest";
import { getLedgerEntries, getPayoutSummary } from "@/lib/seller/payouts/data";

describe("seller payouts — payout and ledger", () => {
  it("payout summary has positive lifetime revenue", () => {
    const p = getPayoutSummary();
    expect(p.lifetime).toBeGreaterThan(0);
    expect(p.lifetimeOrders).toBeGreaterThan(0);
  });

  it("ledger entries include at least one sale and one payout entry", () => {
    const entries = getLedgerEntries();
    expect(entries.some((e) => e.type === "sale")).toBe(true);
    expect(entries.some((e) => e.type === "payout")).toBe(true);
  });

  it("payout entries have positive amounts, fee entries are negative", () => {
    const entries = getLedgerEntries();
    for (const e of entries.filter((e) => e.type === "payout")) {
      expect(e.amount).toBeGreaterThan(0);
    }
    for (const e of entries.filter((e) => e.type === "fee")) {
      expect(e.amount).toBeLessThan(0);
    }
  });
});
