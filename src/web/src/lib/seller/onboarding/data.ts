// web/src/lib/seller/onboarding/data.ts
import type {
  FirstMonthKpi,
  FirstOrderKpi,
  TopSeller,
} from "@/lib/seller/onboarding/types";

const DAY_ONE_STATS: { label: string; value: string; subject: string }[] = [
  { label: "Sales · today", value: "$0.00", subject: "no activity yet" },
  { label: "Orders · today", value: "0", subject: "no activity yet" },
  { label: "Visits · today", value: "14", subject: "mostly you :)" },
];
export function getDayOneStats(): {
  label: string;
  value: string;
  subject: string;
}[] {
  return DAY_ONE_STATS;
}

const FIRST_ORDER_STATS: FirstOrderKpi[] = [
  {
    label: "Sales · today",
    value: "$86",
    delta: "first sale!",
    spark: [0, 0, 0, 0, 0, 0, 1],
  },
  {
    label: "Orders · today",
    value: "1",
    delta: "new",
    spark: [0, 0, 0, 0, 0, 0, 1],
  },
  {
    label: "Visits · today",
    value: "142",
    delta: "8× yesterday",
    spark: [0.1, 0.15, 0.1, 0.2, 0.3, 0.6, 0.95],
  },
  {
    label: "Followers",
    value: "14",
    delta: "+5 today",
    spark: [0.1, 0.2, 0.2, 0.4, 0.45, 0.7, 0.95],
  },
];
export function getFirstOrderStats(): FirstOrderKpi[] {
  return FIRST_ORDER_STATS;
}

const FIRST_MONTH_KPIS: FirstMonthKpi[] = [
  {
    label: "Revenue",
    value: "$2,148",
    delta: "+ first month",
    spark: [0.05, 0.1, 0.2, 0.25, 0.4, 0.55, 0.7, 0.85],
  },
  {
    label: "Orders",
    value: "23",
    delta: "avg $93.39",
    spark: [0.05, 0.1, 0.2, 0.3, 0.45, 0.6, 0.7, 0.9],
  },
  {
    label: "Conversion",
    value: "3.1%",
    delta: "vs 2.4% benchmark",
    spark: [0.3, 0.4, 0.5, 0.55, 0.6, 0.7, 0.75, 0.85],
  },
  {
    label: "Repeat buyers",
    value: "4",
    delta: "17% of orders",
    spark: [0, 0, 0.1, 0.2, 0.3, 0.4, 0.5, 0.7],
  },
];
export function getFirstMonthKpis(): FirstMonthKpi[] {
  return FIRST_MONTH_KPIS;
}

const FIRST_MONTH_SERIES: number[] = [
  0, 0, 0, 86, 0, 0, 0, 152, 78, 110, 0, 64, 230, 86, 110, 0, 178, 220, 64, 152,
  86, 320, 110, 152, 64, 86, 230, 110, 320, 86,
];
export function getFirstMonthSeries(): number[] {
  return FIRST_MONTH_SERIES;
}

const FIRST_MONTH_TOP_SELLERS: TopSeller[] = [
  {
    sku: "MC-VS-001",
    name: "Persimmon vase",
    soldLabel: "14 sold",
    revenue: 1204,
    tone: "clay",
  },
  {
    sku: "MC-BW-014",
    name: "Forest bowl, lg.",
    soldLabel: "6 sold",
    revenue: 384,
    tone: "sage",
  },
  {
    sku: "MC-TB-007",
    name: "Cream tumbler set",
    soldLabel: "4 sold",
    revenue: 192,
    tone: "cream",
  },
];
export function getFirstMonthTopSellers(): TopSeller[] {
  return FIRST_MONTH_TOP_SELLERS;
}
