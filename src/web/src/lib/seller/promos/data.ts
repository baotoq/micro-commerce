// web/src/lib/seller/promos/data.ts
import type { PromoCode, PromoStat, PromoTab } from "@/lib/seller/promos/types";

const PROMOS: PromoCode[] = [
  {
    code: "SPRING20",
    what: "20% off · sitewide",
    window: "Apr 1 → May 15",
    redemptions: 142,
    drivenRevenue: 1842,
    status: "Active",
    tone: "good",
  },
  {
    code: "WELCOME10",
    what: "$10 off · first order $40+",
    window: "Always · 1 per buyer",
    redemptions: 38,
    drivenRevenue: 612,
    status: "Active",
    tone: "good",
  },
  {
    code: "STUDIO15",
    what: "15% off · followers only",
    window: "Apr 22 → May 06",
    redemptions: 17,
    drivenRevenue: 286,
    status: "Active",
    tone: "good",
    highlight: true,
  },
  {
    code: "BLOOM",
    what: "Free ship · $80+",
    window: "Mar 1 → Apr 12",
    redemptions: 84,
    drivenRevenue: 0,
    status: "Ended",
    tone: "mute",
  },
  {
    code: "FRIENDS",
    what: "15% off · sitewide",
    window: "Drafted",
    redemptions: 0,
    drivenRevenue: 0,
    status: "Draft",
    tone: "mute",
  },
];
export function getPromos(): PromoCode[] {
  return PROMOS;
}

const PROMO_STATS: PromoStat[] = [
  {
    label: "Driven revenue",
    value: "$2,740",
    sub: "Last 30 days",
    spark: [0.1, 0.2, 0.3, 0.45, 0.6, 0.55, 0.7, 0.85],
  },
  {
    label: "Redemptions",
    value: "281",
    sub: "14% of orders",
    spark: [0.2, 0.25, 0.4, 0.45, 0.55, 0.6, 0.7, 0.8],
  },
  {
    label: "Avg. discount",
    value: "$9.74",
    sub: "per redemption",
    spark: [0.3, 0.32, 0.36, 0.4, 0.42, 0.45, 0.5, 0.52],
  },
  {
    label: "New buyers",
    value: "38",
    sub: "from WELCOME10",
    spark: [0, 0.1, 0.15, 0.2, 0.3, 0.4, 0.5, 0.7],
  },
];
export function getPromoStats(): PromoStat[] {
  return PROMO_STATS;
}

const PROMO_TABS: PromoTab[] = [
  { label: "Promotions", count: 5, on: true },
  { label: "Automatic", count: 1 },
  { label: "Gift cards", count: 0 },
];
export function getPromoTabs(): PromoTab[] {
  return PROMO_TABS;
}
