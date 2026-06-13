// web/src/lib/seller/orders/data.ts
//
// AUTH RULE (Keycloak): these loaders are cached (`"use cache"`) and therefore
// anonymous — calling auth()/getAccessToken() inside `"use cache"` is illegal.
// A loader is either cached+anonymous or uncached+authenticated, never both.
// The underlying read endpoints are public; authenticated writes live in the
// uncached fetchers under lib/catalog/*.ts.
import { cacheTag } from "next/cache";
import {
  fetchOrderByNumber,
  fetchOrderCounts,
  fetchOrders,
  type OrderCountsDto,
  type OrderDetailDto,
  type OrderInboxRowDto,
} from "@/lib/catalog/orders";
import { DEMO_NOW } from "@/lib/seller/demo-clock";
import {
  formatAge,
  formatPlacedLabel,
  formatRelative,
  shortName,
  shortNamePanel,
  statusToDisplay,
} from "@/lib/seller/orders/format";
import type {
  Order,
  OrderDetail,
  OrderDetailFull,
  OrderInboxRow,
  OrderInboxSummary,
  OrderInboxTab,
  OrderStatus,
  ShippingOption,
} from "@/lib/seller/orders/types";

const RESTOCK_OPTIONS = ["Yes", "No", "Damage"];

// Universe of customer tags shown in the internal-note panel. The active subset
// (which tags this customer actually carries) comes from the Customers domain.
const CUSTOMER_TAGS = ["VIP", "Repeat buyer", "Gift"];

/** 1 → "1st", 2 → "2nd", 3 → "3rd", 4 → "4th" … */
function ordinal(n: number): string {
  const mod100 = n % 100;
  if (mod100 >= 11 && mod100 <= 13) return `${n}th`;
  switch (n % 10) {
    case 1:
      return `${n}st`;
    case 2:
      return `${n}nd`;
    case 3:
      return `${n}rd`;
    default:
      return `${n}th`;
  }
}

function rowToInbox(dto: OrderInboxRowDto): OrderInboxRow {
  const placedAt = new Date(dto.placedAt);
  const { label, tone } = statusToDisplay(dto.status);
  return {
    id: `#${dto.number}`,
    placedLabel: formatPlacedLabel(placedAt, DEMO_NOW),
    customer: shortName(dto.customerName),
    city: dto.cityState,
    items: dto.itemsSummary,
    qty: dto.qty,
    total: dto.total,
    ship: dto.shippingMethod,
    status: label,
    tone,
    age: formatAge(placedAt, DEMO_NOW),
    starred: dto.starred,
  };
}

export async function getOrderInbox(): Promise<OrderInboxRow[]> {
  "use cache";
  cacheTag("orders");
  const page = await fetchOrders({ tab: "all" });
  return page.items.map(rowToInbox);
}

export async function getOrderInboxTabs(): Promise<OrderInboxTab[]> {
  "use cache";
  cacheTag("orders");
  const counts = await fetchOrderCounts();
  return [
    { label: "All", count: counts.all },
    { label: "Needs action", count: counts.needsAction, on: true },
    { label: "Packed", count: counts.packed },
    { label: "Shipped", count: counts.shipped },
    { label: "Delivered", count: counts.delivered },
    { label: "Refund / cancel", count: counts.refundOrCancel },
  ];
}

export async function getOrderInboxSummary(): Promise<OrderInboxSummary> {
  "use cache";
  cacheTag("orders");
  const counts: OrderCountsDto = await fetchOrderCounts();
  return {
    totalLifetime: counts.all,
    needAction: counts.needsAction,
  };
}

const RECENT_STATUS: Record<string, OrderStatus> = {
  new: "paid",
  packed: "paid",
  shipped: "fulfilled",
  delivered: "fulfilled",
  "refund-requested": "refunded",
  cancelled: "refunded",
};

export async function getRecentOrders(): Promise<Order[]> {
  "use cache";
  cacheTag("orders");
  const page = await fetchOrders({ tab: "all", limit: 5 });
  return page.items.slice(0, 5).map((dto) => ({
    id: `#${dto.number}`,
    customer: dto.customerName,
    items: dto.qty,
    total: dto.total,
    status: RECENT_STATUS[dto.status] ?? "pending",
    placedAt: dto.placedAt,
  }));
}

/** Header status + tone. Mixed line fulfillment reads as "Partially fulfilled". */
function detailStatus(dto: OrderDetailDto): {
  status: string;
  statusTone: OrderDetailFull["statusTone"];
} {
  const hasShipped = dto.lines.some((l) => l.status === "shipped");
  const hasAwaiting = dto.lines.some((l) => l.status === "awaiting");
  if (hasShipped && hasAwaiting) {
    return { status: "Partially fulfilled", statusTone: "warn" };
  }
  const { label, tone } = statusToDisplay(dto.status);
  return { status: label, statusTone: tone };
}

function detailToFull(dto: OrderDetailDto): OrderDetailFull {
  const placedAt = new Date(dto.placedAt);
  const { status, statusTone } = detailStatus(dto);

  const fulfillments = dto.lines.map((l) => ({
    idx: l.idx,
    of: l.of,
    status: l.status as "shipped" | "awaiting",
    productName: l.productName,
    productSubtitle: `SKU ${l.sku} · qty ${l.qty} · $${l.unitPrice.toFixed(2)}`,
    productTone: l.tone,
    qty: l.qty,
    price: l.unitPrice,
    ...(l.tracking ? { tracking: l.tracking } : {}),
    ...(l.restockNote ? { restockNote: l.restockNote } : {}),
  }));

  const skuToName = new Map(dto.lines.map((l) => [l.sku, l.productName]));

  const refund: OrderDetailFull["refund"] = dto.refund
    ? {
        refundable: dto.refund.refundable,
        itemsCount: dto.refund.itemsCount,
        items: dto.refund.items.map((it) => ({
          name: skuToName.get(it.sku) ?? it.sku,
          qty: it.qty,
          price: it.amount,
          selected: it.selected,
          ...(it.partialAmount != null ? { partial: it.partialAmount } : {}),
        })),
        reason: dto.refund.reason,
        restockOptions: RESTOCK_OPTIONS,
        restockSelected: dto.refund.restockChoice,
        total: dto.refund.total,
        lastFour: dto.refund.lastFour,
      }
    : {
        refundable: 0,
        itemsCount: 0,
        items: [],
        reason: "",
        restockOptions: RESTOCK_OPTIONS,
        restockSelected: "Yes",
        total: 0,
        lastFour: "",
      };

  const c = dto.customer;
  const lifetimeOrdersLabel = `${ordinal(c.lifetimeOrderCount)} order · $${c.lifetimeSpend} lifetime`;

  return {
    id: `#${dto.number}`,
    status,
    statusTone,
    customerShort: shortName(c.name),
    age: formatRelative(placedAt, DEMO_NOW),
    fulfillments,
    refund,
    customer: {
      name: c.name,
      shortName: shortNamePanel(c.name),
      lifetimeOrdersLabel,
      ship: { line1: c.shipLine1, line2: c.shipLine2 },
      billSameAsShip: c.billSameAsShip,
      email: c.email,
    },
    summary: {
      subtotal: dto.summary.subtotal,
      itemsCount: dto.summary.itemsCount,
      shipping: dto.summary.shipping,
      tax: dto.summary.tax,
      paid: dto.summary.paid,
      feePct: dto.summary.feePct,
      fee: dto.summary.fee,
      labelCarrier: dto.summary.labelCarrier ?? "",
      labelCost: dto.summary.labelCost,
      net: dto.summary.net,
      discountCode: dto.summary.discountCode,
      discountAmount: dto.summary.discountAmount,
    },
    internalNote: dto.internalNote,
    customerTags: CUSTOMER_TAGS,
    customerTagsActive: c.tags,
    timeline: dto.timeline.map((t) => ({
      icon: t.icon,
      title: t.title,
      sub: t.sub,
      when: formatRelative(new Date(t.occurredAt), placedAt),
      ...(t.highlight ? { on: true } : {}),
      ...(t.tone === "warn" ? { tone: "warn" as const } : {}),
    })),
    outstandingProductName: dto.outstandingProductName ?? "",
  };
}

export async function getOrderDetailFull(
  id: string,
): Promise<OrderDetailFull | null> {
  "use cache";
  cacheTag("orders");
  const number = Number(id);
  if (!Number.isInteger(number)) return null;
  const dto = await fetchOrderByNumber(number);
  if (!dto) return null;
  return detailToFull(dto);
}

// ── Static mocks — the pack page stays a fixed visual ──

const ORDER_DETAIL_1001: OrderDetail = {
  id: "#1001",
  customer: "Sasha Leblanc",
  shortCustomer: "Sasha L.",
  productName: "Persimmon vase",
  productSubtitle: "Glazed terra · qty 1",
  qty: 1,
  subtotal: 86,
  shippingLabel: "USPS Ground",
  shippingCost: 0,
  customerPaid: 86,
  feePct: 4,
  fee: 3.44,
  net: 82.56,
  shipTo: {
    name: "Sasha Leblanc",
    line1: "820 Sutter St #4B",
    cityState: "SF CA 94109",
  },
  customerNote: "So excited — please pack carefully, this is for my mom.",
};
export function getOrderDetail(id: string): OrderDetail | null {
  if (id === "1001") return ORDER_DETAIL_1001;
  return null;
}

const SHIPPING_OPTIONS: ShippingOption[] = [
  {
    label: "USPS Priority · 1–3 days",
    sub: "Tracked · $50 insured",
    price: 9.84,
    selected: true,
  },
  {
    label: "USPS Ground Advantage",
    sub: "2–5 days · tracked",
    price: 6.52,
    selected: false,
  },
  {
    label: "UPS Ground",
    sub: "3–4 days · pickup avail.",
    price: 11.2,
    selected: false,
  },
];
export function getShippingOptions(): ShippingOption[] {
  return SHIPPING_OPTIONS;
}
