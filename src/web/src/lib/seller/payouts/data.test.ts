import { afterEach, describe, expect, it, vi } from "vitest";
import { money } from "@/lib/money";

vi.mock("server-only", () => ({}));
vi.mock("next/cache", () => ({ cacheTag: vi.fn() }));
vi.mock("@/lib/catalog/payouts", () => ({
  fetchPayoutSummary: vi.fn(),
  fetchLedger: vi.fn(),
  fetchPayouts: vi.fn(),
}));

import type {
  LedgerEntryDto,
  PayoutDto,
  PayoutSummaryDto,
} from "@/lib/catalog/payouts";
import * as payoutsClient from "@/lib/catalog/payouts";
import { getLedgerEntries, getPayoutSummary } from "@/lib/seller/payouts/data";

function makeSummaryDto(
  overrides: Partial<PayoutSummaryDto> = {},
): PayoutSummaryDto {
  return {
    available: 253.44,
    pending: 372.0,
    nextSend: 253.44,
    // 2026-04-15 in America/Los_Angeles is a Wednesday.
    availableSendsOn: "2026-04-15T07:00:00Z",
    lifetime: 2148.36,
    lifetimeOrders: 23,
    lifetimeRange: "Mar 12 → Apr 8",
    ...overrides,
  };
}

function makePayoutDto(overrides: Partial<PayoutDto> = {}): PayoutDto {
  return {
    id: "11111111-1111-1111-1111-111111111111",
    sentAt: "2026-04-08T07:00:00Z",
    amount: 1284.62,
    period: "Mar 12 – Apr 8",
    destination: "Chase ···· 8847",
    isPending: false,
    ...overrides,
  };
}

function makeLedgerDto(
  overrides: Partial<LedgerEntryDto> = {},
): LedgerEntryDto {
  return {
    occurredAt: "2026-04-08T07:00:00Z",
    label: "Sale – Persimmon vase",
    subject: "Order #1042 · Apr 8",
    amount: 86.0,
    kind: "sale",
    ...overrides,
  };
}

describe("getPayoutSummary", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("maps PayoutSummaryDto + latest payout to the PayoutSummary view model", async () => {
    vi.mocked(payoutsClient.fetchPayoutSummary).mockResolvedValueOnce(
      makeSummaryDto(),
    );
    vi.mocked(payoutsClient.fetchPayouts).mockResolvedValueOnce({
      items: [makePayoutDto()],
      total: 1,
      page: 1,
      pageSize: 20,
    });

    const summary = await getPayoutSummary();

    // available formats to "$253.44"
    expect(summary.available).toBe(253.44);
    expect(money(summary.available)).toBe("$253.44");
    // sends on the next Wednesday, formatted in Pacific time
    expect(summary.availableSendsOn).toBe("Wed, Apr 15");
    expect(summary.availableFromOrders).toBe(3);
    expect(summary.lifetime).toBe(2148.36);
    expect(summary.lifetimeOrders).toBe(23);
    expect(summary.lifetimeRange).toBe("Mar 12 → Apr 8");
    // last payout pulled from the latest payout row
    expect(summary.lastPayout).toBe(1284.62);
    expect(money(summary.lastPayout)).toBe("$1,284.62");
    expect(summary.lastPayoutSentLabel).toBe("Sent · arriving Wed");
  });

  it("falls back to a zero last payout when there are no payouts yet", async () => {
    vi.mocked(payoutsClient.fetchPayoutSummary).mockResolvedValueOnce(
      makeSummaryDto(),
    );
    vi.mocked(payoutsClient.fetchPayouts).mockResolvedValueOnce({
      items: [],
      total: 0,
      page: 1,
      pageSize: 20,
    });

    const summary = await getPayoutSummary();

    expect(summary.lastPayout).toBe(0);
    expect(summary.lastPayoutSentLabel).toBe("No payouts yet");
  });
});

describe("getLedgerEntries", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("maps LedgerEntryDto rows to LedgerEntry view models", async () => {
    vi.mocked(payoutsClient.fetchLedger).mockResolvedValueOnce({
      items: [
        makeLedgerDto(),
        makeLedgerDto({
          label: "Fee – Persimmon vase",
          subject: "Order #1042 · Apr 8",
          amount: -3.44,
          kind: "fee",
        }),
        makeLedgerDto({
          occurredAt: "2026-04-01T07:00:00Z",
          label: "Payout – Week of Mar 25",
          subject: "Chase ···· 8847 · direct deposit",
          amount: -247.2,
          kind: "payout",
        }),
      ],
      total: 3,
      page: 1,
      pageSize: 20,
    });

    const entries = await getLedgerEntries();

    expect(entries).toHaveLength(3);

    expect(entries[0].date).toBe("Apr 8");
    expect(entries[0].label).toBe("Sale – Persimmon vase");
    expect(entries[0].subject).toBe("Order #1042 · Apr 8");
    expect(entries[0].amount).toBe(86.0);
    expect(entries[0].type).toBe("sale");

    expect(entries[1].label).toBe("Fee – Persimmon vase");
    expect(entries[1].amount).toBe(-3.44);
    expect(entries[1].type).toBe("fee");

    expect(entries[2].date).toBe("Apr 1");
    expect(entries[2].label).toBe("Payout – Week of Mar 25");
    expect(entries[2].amount).toBe(-247.2);
    expect(entries[2].type).toBe("payout");
  });

  it("maps a null subject to an empty string and label-kind entries to fees", async () => {
    vi.mocked(payoutsClient.fetchLedger).mockResolvedValueOnce({
      items: [
        makeLedgerDto({
          label: "Shipping label · USPS",
          subject: null,
          amount: -9.84,
          kind: "label",
        }),
      ],
      total: 1,
      page: 1,
      pageSize: 20,
    });

    const entries = await getLedgerEntries();

    expect(entries[0].subject).toBe("");
    expect(entries[0].type).toBe("fee");
    expect(entries[0].amount).toBe(-9.84);
  });
});
