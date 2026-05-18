// web/src/lib/seller/promos/types.ts
export type PromoStatus = "Active" | "Ended" | "Draft";

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
