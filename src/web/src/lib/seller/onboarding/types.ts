// web/src/lib/seller/onboarding/types.ts
export type FirstOrderKpi = {
  label: string;
  value: string;
  delta: string;
  spark: number[];
};

export type FirstMonthKpi = {
  label: string;
  value: string;
  delta: string;
  spark: number[];
};

export type TopSeller = {
  sku: string;
  name: string;
  soldLabel: string;
  revenue: number;
  tone: string;
};
