// web/src/lib/seller/orders/data.test.ts
import { afterEach, describe, expect, it, vi } from "vitest";

vi.mock("server-only", () => ({}));
vi.mock("next/cache", () => ({ cacheTag: vi.fn() }));
vi.mock("@/lib/catalog/orders", () => ({
  fetchOrders: vi.fn(),
  fetchOrderCounts: vi.fn(),
  fetchOrderByNumber: vi.fn(),
}));

import type {
  OrderCountsDto,
  OrderDetailDto,
  OrderInboxRowDto,
} from "@/lib/catalog/orders";
import * as ordersClient from "@/lib/catalog/orders";
import {
  getOrderDetail,
  getOrderDetailFull,
  getOrderInbox,
  getOrderInboxSummary,
  getOrderInboxTabs,
  getRecentOrders,
  getShippingOptions,
} from "@/lib/seller/orders/data";

function makeRow(overrides: Partial<OrderInboxRowDto> = {}): OrderInboxRowDto {
  return {
    number: 1042,
    placedAt: "2026-04-08T21:14:00Z",
    customerName: "Sasha Leblanc",
    cityState: "San Francisco, CA",
    itemsSummary: "Persimmon vase, Ash budstem",
    qty: 2,
    total: 152,
    shippingMethod: "USPS Priority",
    status: "new",
    starred: true,
    ...overrides,
  };
}

function makeCounts(overrides: Partial<OrderCountsDto> = {}): OrderCountsDto {
  return {
    all: 46,
    needsAction: 4,
    new: 3,
    packed: 2,
    shipped: 18,
    delivered: 21,
    refundOrCancel: 2,
    lifetimeRevenue: 5280,
    ...overrides,
  };
}

function makeDetail1042(): OrderDetailDto {
  const placedAt = "2026-04-08T21:14:00Z";
  const minus = (mins: number) =>
    new Date(new Date(placedAt).getTime() - mins * 60_000).toISOString();
  return {
    number: 1042,
    status: "new",
    placedAt,
    lines: [
      {
        idx: 1,
        of: 2,
        status: "shipped",
        sku: "MC-VS-001",
        productName: "Persimmon vase",
        tone: "clay",
        qty: 1,
        unitPrice: 86,
        tracking: "USPS · 9405 5036 9930 0124 2317",
        restockNote: null,
      },
      {
        idx: 2,
        of: 2,
        status: "awaiting",
        sku: "MC-AB-002",
        productName: "Ash budstem",
        tone: "rust",
        qty: 1,
        unitPrice: 66,
        tracking: null,
        restockNote: "back in stock Tue",
      },
    ],
    refund: {
      refundable: 152,
      itemsCount: 2,
      items: [
        {
          sku: "MC-VS-001",
          qty: 1,
          amount: 86,
          selected: false,
          partialAmount: null,
        },
        {
          sku: "MC-AB-002",
          qty: 1,
          amount: 66,
          selected: true,
          partialAmount: 30,
        },
      ],
      reason: "Item arrived chipped",
      restockChoice: "Damage",
      total: 30,
      lastFour: "4421",
    },
    customer: {
      name: "Sasha Leblanc",
      email: "sasha.l@gmail.com",
      shipLine1: "820 Sutter St · #4B",
      shipLine2: "San Francisco, CA 94109",
      billSameAsShip: true,
      lifetimeOrderCount: 3,
      lifetimeSpend: 284,
      tags: ["VIP", "Repeat buyer"],
    },
    summary: {
      subtotal: 152,
      itemsCount: 2,
      shipping: 0,
      tax: 0,
      paid: 152,
      feePct: 4,
      fee: 6.08,
      labelCarrier: "USPS",
      labelCost: 9.84,
      net: 136.08,
    },
    internalNote: "Held until budstem restocks Tue. Sasha OK with split.",
    timeline: [
      {
        icon: "check",
        title: "Order placed",
        sub: "2 items · $152.00 paid via Visa · 4421",
        occurredAt: minus(120),
        tone: null,
        highlight: true,
      },
      {
        icon: "box",
        title: "Persimmon vase packed",
        sub: "Box S · 1lb 4oz",
        occurredAt: minus(60),
        tone: null,
        highlight: false,
      },
      {
        icon: "truck",
        title: "Persimmon vase shipped",
        sub: "USPS Priority · 1–3 days",
        occurredAt: minus(52),
        tone: null,
        highlight: false,
      },
      {
        icon: "chat",
        title: "Note from Sasha",
        sub: "“No rush on the budstem — ship together if it’s faster!”",
        occurredAt: minus(14),
        tone: null,
        highlight: false,
      },
      {
        icon: "info",
        title: "Ash budstem oversold",
        sub: "Restock arrives Tue · auto-fulfill on",
        occurredAt: minus(8),
        tone: "warn",
        highlight: false,
      },
    ],
    outstandingProductName: "Ash budstem",
  };
}

afterEach(() => {
  vi.clearAllMocks();
});

describe("getOrderInbox", () => {
  it("maps OrderInboxRowDto rows to inbox view rows", async () => {
    vi.mocked(ordersClient.fetchOrders).mockResolvedValueOnce({
      items: [makeRow()],
      total: 1,
      page: 1,
      pageSize: 50,
    });

    const rows = await getOrderInbox();

    expect(rows).toHaveLength(1);
    expect(rows[0].id).toBe("#1042");
    expect(rows[0].customer).toBe("Sasha L.");
    expect(rows[0].city).toBe("San Francisco, CA");
    expect(rows[0].items).toBe("Persimmon vase, Ash budstem");
    expect(rows[0].qty).toBe(2);
    expect(rows[0].total).toBe(152);
    expect(rows[0].ship).toBe("USPS Priority");
    expect(rows[0].status).toBe("New");
    expect(rows[0].tone).toBe("warn");
    expect(rows[0].placedLabel).toBe("Today · 2:14 PM");
    expect(rows[0].age).toBe("2h");
    expect(rows[0].starred).toBe(true);
  });

  it("maps status + tone per wire key", async () => {
    vi.mocked(ordersClient.fetchOrders).mockResolvedValueOnce({
      items: [
        makeRow({ number: 1035, status: "delivered", starred: false }),
        makeRow({ number: 1036, status: "refund-requested", starred: false }),
      ],
      total: 2,
      page: 1,
      pageSize: 50,
    });

    const rows = await getOrderInbox();
    expect(rows[0].status).toBe("Delivered");
    expect(rows[0].tone).toBe("good");
    expect(rows[1].status).toBe("Refund req.");
    expect(rows[1].tone).toBe("bad");
  });
});

describe("getOrderInboxTabs", () => {
  it("maps OrderCountsDto to 6 tabs with Needs action active", async () => {
    vi.mocked(ordersClient.fetchOrderCounts).mockResolvedValueOnce(
      makeCounts(),
    );

    const tabs = await getOrderInboxTabs();

    expect(tabs).toHaveLength(6);
    const all = tabs.find((t) => t.label === "All");
    expect(all?.count).toBe(46);
    const needs = tabs.find((t) => t.label === "Needs action");
    expect(needs?.count).toBe(4);
    expect(needs?.on).toBe(true);
    expect(tabs.find((t) => t.label === "Packed")?.count).toBe(2);
    expect(tabs.find((t) => t.label === "Shipped")?.count).toBe(18);
    expect(tabs.find((t) => t.label === "Delivered")?.count).toBe(21);
    expect(tabs.find((t) => t.label === "Refund / cancel")?.count).toBe(2);
  });
});

describe("getOrderInboxSummary", () => {
  it("maps lifetime + needs-action counts", async () => {
    vi.mocked(ordersClient.fetchOrderCounts).mockResolvedValueOnce(
      makeCounts(),
    );

    const s = await getOrderInboxSummary();
    expect(s.totalLifetime).toBe(46);
    expect(s.needAction).toBe(4);
  });
});

describe("getRecentOrders", () => {
  it("maps the most recent inbox rows to legacy Order rows", async () => {
    vi.mocked(ordersClient.fetchOrders).mockResolvedValueOnce({
      items: [makeRow({ number: 1042, customerName: "Sasha Leblanc", qty: 2 })],
      total: 1,
      page: 1,
      pageSize: 5,
    });

    const orders = await getRecentOrders();
    expect(orders).toHaveLength(1);
    expect(orders[0].id).toBe("#1042");
    expect(orders[0].customer).toBe("Sasha Leblanc");
    expect(orders[0].items).toBe(2);
    expect(orders[0].total).toBe(152);
  });
});

describe("getOrderDetailFull", () => {
  it("returns null for an unknown order number", async () => {
    vi.mocked(ordersClient.fetchOrderByNumber).mockResolvedValueOnce(null);
    expect(await getOrderDetailFull("9999")).toBeNull();
  });

  it("maps #1042 to the partial-fulfillment detail view model", async () => {
    vi.mocked(ordersClient.fetchOrderByNumber).mockResolvedValueOnce(
      makeDetail1042(),
    );

    const d = await getOrderDetailFull("1042");
    expect(d).not.toBeNull();
    if (!d) return;

    expect(d.id).toBe("#1042");
    expect(d.status).toBe("Partially fulfilled");
    expect(d.statusTone).toBe("warn");
    expect(d.customerShort).toBe("Sasha L.");

    // fulfillments
    expect(d.fulfillments).toHaveLength(2);
    expect(d.fulfillments[0].status).toBe("shipped");
    expect(d.fulfillments[0].productName).toBe("Persimmon vase");
    expect(d.fulfillments[0].productSubtitle).toBe(
      "SKU MC-VS-001 · qty 1 · $86.00",
    );
    expect(d.fulfillments[0].productTone).toBe("clay");
    expect(d.fulfillments[0].tracking).toBe("USPS · 9405 5036 9930 0124 2317");
    expect(d.fulfillments[1].status).toBe("awaiting");
    expect(d.fulfillments[1].productName).toBe("Ash budstem");
    expect(d.fulfillments[1].productSubtitle).toBe(
      "SKU MC-AB-002 · qty 1 · $66.00",
    );
    expect(d.fulfillments[1].restockNote).toBe("back in stock Tue");
    expect(d.outstandingProductName).toBe("Ash budstem");

    // refund
    expect(d.refund.refundable).toBe(152);
    expect(d.refund.itemsCount).toBe(2);
    expect(d.refund.items).toHaveLength(2);
    expect(d.refund.items[0].name).toBe("Persimmon vase");
    expect(d.refund.items[1].name).toBe("Ash budstem");
    expect(d.refund.items[1].selected).toBe(true);
    expect(d.refund.items[1].partial).toBe(30);
    expect(d.refund.reason).toBe("Item arrived chipped");
    expect(d.refund.restockOptions).toEqual(["Yes", "No", "Damage"]);
    expect(d.refund.restockSelected).toBe("Damage");
    expect(d.refund.total).toBe(30);
    expect(d.refund.lastFour).toBe("4421");

    // customer
    expect(d.customer.name).toBe("Sasha Leblanc");
    expect(d.customer.shortName).toBe("Sasha L");
    expect(d.customer.lifetimeOrdersLabel).toBe("3rd order · $284 lifetime");
    expect(d.customer.ship.line1).toBe("820 Sutter St · #4B");
    expect(d.customer.ship.line2).toBe("San Francisco, CA 94109");
    expect(d.customer.billSameAsShip).toBe(true);
    expect(d.customer.email).toBe("sasha.l@gmail.com");
    expect(d.customerTagsActive).toEqual(["VIP", "Repeat buyer"]);

    // summary
    expect(d.summary.subtotal).toBe(152);
    expect(d.summary.paid).toBe(152);
    expect(d.summary.fee).toBe(6.08);
    expect(d.summary.labelCarrier).toBe("USPS");
    expect(d.summary.labelCost).toBe(9.84);
    expect(d.summary.net).toBe(136.08);

    // internal note + timeline
    expect(d.internalNote).toBe(
      "Held until budstem restocks Tue. Sasha OK with split.",
    );
    expect(d.timeline).toHaveLength(5);
    expect(d.timeline[0].when).toBe("2h ago");
    expect(d.timeline[0].on).toBe(true);
    expect(d.timeline[2].when).toBe("52m ago");
    expect(d.timeline[4].when).toBe("8m ago");
    expect(d.timeline[4].tone).toBe("warn");
  });
});

describe("static mocks left intact (pack page stays static)", () => {
  it("getOrderDetail still returns the #1001 static mock synchronously", () => {
    const d = getOrderDetail("1001");
    expect(d).not.toBeNull();
    if (!d) return;
    expect(d.id).toBe("#1001");
    expect(d.customer).toBe("Sasha Leblanc");
    expect(d.subtotal).toBe(86);
    expect(d.feePct).toBe(4);
    expect(getOrderDetail("9999")).toBeNull();
  });

  it("getShippingOptions still returns the 3 static options synchronously", () => {
    const opts = getShippingOptions();
    expect(opts).toHaveLength(3);
    expect(opts.filter((o) => o.selected)).toHaveLength(1);
  });
});
