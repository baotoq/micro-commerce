// AUTH RULE (Keycloak): these loaders are cached (`"use cache"`) and therefore
// anonymous — calling auth()/getAccessToken() inside `"use cache"` is illegal.
// A loader is either cached+anonymous or uncached+authenticated, never both.
// The underlying read endpoints are public; authenticated writes live in the
// uncached fetchers under lib/catalog/*.ts.
import { cacheTag } from "next/cache";
import {
  fetchLedger,
  fetchPayoutSummary,
  fetchPayouts,
  type LedgerEntryDto,
} from "@/lib/catalog/payouts";
import type { LedgerEntry, PayoutSummary } from "@/lib/seller/payouts/types";

const TZ = "America/Los_Angeles";

// "Wed, Apr 15" — weekday + month + day in Pacific time so the label renders
// identically regardless of the host machine's local zone.
const SENDS_ON_FMT = new Intl.DateTimeFormat("en-US", {
  timeZone: TZ,
  weekday: "short",
  month: "short",
  day: "numeric",
});

// "Apr 8" — month + day for ledger row dates, in Pacific time.
const LEDGER_DATE_FMT = new Intl.DateTimeFormat("en-US", {
  timeZone: TZ,
  month: "short",
  day: "numeric",
});

// Display-only count of orders feeding the next payout. Not modeled on the
// summary DTO; fixed at 3 (the New-order count) per the demo drift policy.
const AVAILABLE_FROM_ORDERS = 3;

function ledgerTypeFromKind(kind: LedgerEntryDto["kind"]): LedgerEntry["type"] {
  if (kind === "payout") return "payout";
  if (kind === "sale") return "sale";
  // "fee" and "label" both render as a negative mute "Fee" row.
  return "fee";
}

export async function getPayoutSummary(): Promise<PayoutSummary> {
  "use cache";
  cacheTag("payouts");
  const [summary, payouts] = await Promise.all([
    fetchPayoutSummary(),
    fetchPayouts({ limit: 1 }),
  ]);

  const latest = payouts.items[0];

  return {
    lastPayout: latest?.amount ?? 0,
    lastPayoutSentLabel: latest ? "Sent · arriving Wed" : "No payouts yet",
    available: summary.available,
    availableSendsOn: SENDS_ON_FMT.format(new Date(summary.availableSendsOn)),
    availableFromOrders: AVAILABLE_FROM_ORDERS,
    lifetime: summary.lifetime,
    lifetimeOrders: summary.lifetimeOrders,
    lifetimeRange: summary.lifetimeRange,
  };
}

export async function getLedgerEntries(): Promise<LedgerEntry[]> {
  "use cache";
  cacheTag("payouts");
  const page = await fetchLedger();
  return page.items.map((dto) => ({
    date: LEDGER_DATE_FMT.format(new Date(dto.occurredAt)),
    label: dto.label,
    subject: dto.subject ?? "",
    amount: dto.amount,
    type: ledgerTypeFromKind(dto.kind),
  }));
}
