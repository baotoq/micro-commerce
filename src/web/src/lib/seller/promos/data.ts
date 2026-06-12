// AUTH RULE (Keycloak): these loaders are cached (`"use cache"`) and therefore
// anonymous — calling auth()/getAccessToken() inside `"use cache"` is illegal.
// A loader is either cached+anonymous or uncached+authenticated, never both.
// The underlying read endpoints are public; authenticated writes live in the
// uncached fetchers under lib/catalog/*.ts.
import { cacheTag } from "next/cache";
import { fetchPromotionStats, fetchPromotions } from "@/lib/catalog/promotions";
import { formatPromoWhat, formatPromoWindow } from "@/lib/seller/promos/format";
import type {
  PromoCode,
  PromoStat,
  PromoTab,
  PromotionDto,
  PromotionStatus,
} from "@/lib/seller/promos/types";

const SPARK_PLACEHOLDER = [0.1, 0.2, 0.3, 0.45, 0.6, 0.55, 0.7, 0.85];

function capitalizeStatus(
  status: PromotionStatus,
): "Active" | "Draft" | "Ended" {
  if (status === "active") return "Active";
  if (status === "draft") return "Draft";
  return "Ended";
}

function dtoToPromoCode(dto: PromotionDto): PromoCode {
  const status = capitalizeStatus(dto.status);
  return {
    code: dto.code,
    what: formatPromoWhat(dto),
    window: formatPromoWindow({ ...dto, status }),
    redemptions: 0,
    drivenRevenue: 0,
    status,
    tone: status === "Active" ? "good" : "mute",
  };
}

export async function getPromos(): Promise<PromoCode[]> {
  "use cache";
  cacheTag("promotions");
  const page = await fetchPromotions();
  return page.items.map(dtoToPromoCode);
}

export async function getPromoStats(): Promise<PromoStat[]> {
  "use cache";
  cacheTag("promotions");
  const stats = await fetchPromotionStats();
  return [
    {
      label: "Active",
      value: String(stats.active),
      sub: "currently live",
      spark: SPARK_PLACEHOLDER,
    },
    {
      label: "Draft",
      value: String(stats.draft),
      sub: "not yet started",
      spark: SPARK_PLACEHOLDER,
    },
    {
      label: "Ended",
      value: String(stats.ended),
      sub: "past their window",
      spark: SPARK_PLACEHOLDER,
    },
    {
      label: "Total",
      value: String(stats.total),
      sub: "all-time",
      spark: SPARK_PLACEHOLDER,
    },
  ];
}

export async function getPromoTabs(): Promise<PromoTab[]> {
  "use cache";
  cacheTag("promotions");
  const stats = await fetchPromotionStats();
  return [
    { label: "Promotions", count: stats.total, on: true },
    { label: "Automatic", count: 1 },
    { label: "Gift cards", count: 0 },
  ];
}
