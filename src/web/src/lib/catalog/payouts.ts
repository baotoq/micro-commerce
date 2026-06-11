import "server-only";

export interface PayoutSummaryDto {
  available: number;
  pending: number;
  nextSend: number;
  availableSendsOn: string;
  lifetime: number;
  lifetimeOrders: number;
  lifetimeRange: string;
}

export type LedgerEntryKind = "sale" | "fee" | "payout" | "label";

export interface LedgerEntryDto {
  occurredAt: string;
  label: string;
  subject: string | null;
  amount: number;
  kind: LedgerEntryKind;
}

export interface PayoutDto {
  id: string;
  sentAt: string;
  amount: number;
  period: string;
  destination: string;
  isPending: boolean;
}

export interface LedgerPage {
  items: LedgerEntryDto[];
  total: number;
  page: number;
  pageSize: number;
}

export interface PayoutPage {
  items: PayoutDto[];
  total: number;
  page: number;
  pageSize: number;
}

export type LedgerQuery = {
  page?: number;
  limit?: number;
};

export type PayoutQuery = {
  page?: number;
  limit?: number;
};

function apiBase(): string {
  const url = process.env.API_URL;
  if (!url) {
    throw new Error(
      "API_URL env var is not set. Run the app via the Aspire AppHost (`dotnet run --project src/AppHost`).",
    );
  }
  return url.replace(/\/$/, "");
}

export async function fetchPayoutSummary(): Promise<PayoutSummaryDto> {
  const res = await fetch(`${apiBase()}/api/payouts/summary`);
  if (!res.ok)
    throw new Error(`GET /api/payouts/summary failed: ${res.status}`);
  return (await res.json()) as PayoutSummaryDto;
}

export async function fetchLedger(
  query: LedgerQuery = {},
): Promise<LedgerPage> {
  const url = new URL(`${apiBase()}/api/payouts/ledger`);
  if (query.page) url.searchParams.set("page", String(query.page));
  if (query.limit) url.searchParams.set("limit", String(query.limit));

  const res = await fetch(url);
  if (!res.ok) throw new Error(`GET /api/payouts/ledger failed: ${res.status}`);
  return (await res.json()) as LedgerPage;
}

export async function fetchPayouts(
  query: PayoutQuery = {},
): Promise<PayoutPage> {
  const url = new URL(`${apiBase()}/api/payouts`);
  if (query.page) url.searchParams.set("page", String(query.page));
  if (query.limit) url.searchParams.set("limit", String(query.limit));

  const res = await fetch(url);
  if (!res.ok) throw new Error(`GET /api/payouts failed: ${res.status}`);
  return (await res.json()) as PayoutPage;
}
