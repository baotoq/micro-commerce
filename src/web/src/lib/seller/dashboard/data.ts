// web/src/lib/seller/dashboard/data.ts
//
// AUTH RULE (Keycloak): these loaders are cached (`"use cache"`) and therefore
// anonymous — calling auth()/getAccessToken() inside `"use cache"` is illegal.
// A loader is either cached+anonymous or uncached+authenticated, never both.
// The underlying read endpoints are public; authenticated writes live in the
// uncached fetchers under lib/catalog/*.ts.
import { cacheTag } from "next/cache";
import {
  type DashboardSummaryDto,
  fetchDashboardSummary,
  type OrderInboxRowDto,
} from "@/lib/catalog/analytics";
import type { TodayItem } from "@/lib/seller/dashboard/types";
import { shortName } from "@/lib/seller/orders/format";
import type { Order, OrderStatus } from "@/lib/seller/orders/types";

/** Maps an order wire-status key to the dashboard recent-orders status label. */
function wireStatusToOrderStatus(wireKey: string): OrderStatus {
  switch (wireKey) {
    case "new":
      return "paid";
    case "packed":
    case "shipped":
    case "delivered":
      return "fulfilled";
    case "refund-requested":
    case "cancelled":
      return "refunded";
    default:
      return "pending";
  }
}

function dtoToOrder(dto: OrderInboxRowDto): Order {
  return {
    id: `#${dto.number}`,
    customer: shortName(dto.customerName),
    items: dto.qty,
    total: dto.total,
    status: wireStatusToOrderStatus(dto.status),
    placedAt: dto.placedAt,
  };
}

/**
 * The backend ships the first today item as "Pack N orders" plus the count;
 * the dashboard panel renders the full call-to-action label.
 */
function dtoToTodayItems(dto: DashboardSummaryDto): TodayItem[] {
  return dto.todayItems.map((item, i) => {
    if (i === 0) {
      return {
        label: `Pack ${dto.ordersToShip} orders ready to ship`,
        count: item.count ?? undefined,
      };
    }
    return { label: item.label, count: item.count ?? undefined };
  });
}

async function summary(): Promise<DashboardSummaryDto> {
  return fetchDashboardSummary();
}

export async function getTodayItems(): Promise<TodayItem[]> {
  "use cache";
  cacheTag("dashboard");
  const dto = await summary();
  return dtoToTodayItems(dto);
}

export async function getDateLabel(): Promise<string> {
  "use cache";
  cacheTag("dashboard");
  const dto = await summary();
  return dto.dateLabel;
}

export async function getDashboardRecentOrders(): Promise<Order[]> {
  "use cache";
  cacheTag("dashboard");
  const dto = await summary();
  return dto.recentOrders.map(dtoToOrder);
}
