// web/src/lib/seller/promos/types.ts
export type PromoStatus = "Active" | "Ended" | "Draft";

export type PromotionStatus = "active" | "draft" | "ended";

export type PromotionDto = {
  code: string;
  description: string;
  kind: "percentage" | "fixed";
  percentValue: number | null;
  fixedAmount: number | null;
  minOrderAmount: number | null;
  startsAt: string | null;
  endsAt: string | null;
  status: PromotionStatus;
};

export type PromotionStatsDto = {
  active: number;
  draft: number;
  ended: number;
  total: number;
};

export type PromotionInput = {
  code: string;
  description: string;
  kind: "percentage" | "fixed";
  percentValue?: number | null;
  fixedAmount?: number | null;
  minOrderAmount?: number | null;
  startsAt?: string | null;
  endsAt?: string | null;
  activateImmediately: boolean;
};

export type PromoCode = {
  code: string;
  what: string;
  window: string;
  redemptions: number;
  drivenRevenue: number;
  status: PromoStatus;
  tone: "good" | "mute";
  highlight?: boolean;
};

export type PromoStat = {
  label: string;
  value: string;
  sub: string;
  spark: number[];
};

export type PromoTab = {
  label: string;
  count: number;
  on?: boolean;
};
