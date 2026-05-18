// web/src/lib/seller/dashboard/data.ts
import type { TodayItem } from "@/lib/seller/dashboard/types";

const TODAY_ITEMS: TodayItem[] = [
  { label: "Pack 2 orders ready to ship" },
  { label: "Low inventory alert: Ember tea bowl (2 left)" },
  { label: "3 new reviews to moderate" },
  { label: "Restock reminder: Rust mug Nº 04 is out of stock" },
];
export function getTodayItems(): TodayItem[] {
  return TODAY_ITEMS;
}
