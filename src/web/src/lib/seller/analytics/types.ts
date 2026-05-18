// web/src/lib/seller/analytics/types.ts
export type KpiFormat = "currency" | "number" | "percent";

export type KpiPoint = {
  label: string;
  value: number;
  format: KpiFormat;
  delta?: number; // signed percent vs prior period
};

export type RevenuePoint = { day: string; amount: number };

export type SourceBreakdown = { name: string; visits: number; share: number };

export type FunnelStage = { label: string; count: number };

export type TopProduct = {
  sku: string;
  name: string;
  units: number;
  revenue: number;
};
