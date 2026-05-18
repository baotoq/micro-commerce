// web/src/lib/seller/orders/data.test.ts
import { describe, expect, it } from "vitest";
import {
  getOrderDetail,
  getOrderDetailFull,
  getOrderInbox,
  getOrderInboxSummary,
  getOrderInboxTabs,
  getRecentOrders,
  getShippingOptions,
} from "@/lib/seller/orders/data";

describe("seller orders — recent orders", () => {
  it("returns 5 recent orders with unique ids", () => {
    const orders = getRecentOrders();
    expect(orders).toHaveLength(5);
    const ids = orders.map((o) => o.id);
    expect(new Set(ids).size).toBe(5);
    for (const o of orders) expect(o.id.startsWith("#")).toBe(true);
  });
});

describe("seller orders — inbox", () => {
  it("returns 10 inbox rows with #1042 starred at the top", () => {
    const rows = getOrderInbox();
    expect(rows).toHaveLength(10);
    expect(rows[0].id).toBe("#1042");
    expect(rows[0].starred).toBe(true);
    expect(rows[0].customer).toBe("Sasha L.");
    expect(rows[0].items).toBe("Persimmon vase, Ash budstem");
    expect(rows[0].total).toBe(152);
    expect(rows[0].tone).toBe("warn");
  });

  it("inbox tabs total 47 and Needs action is the active tab", () => {
    const tabs = getOrderInboxTabs();
    expect(tabs).toHaveLength(6);
    const all = tabs.find((t) => t.label === "All");
    expect(all?.count).toBe(47);
    const needs = tabs.find((t) => t.label === "Needs action");
    expect(needs?.count).toBe(4);
    expect(needs?.on).toBe(true);
  });

  it("inbox summary reports lifetime + needs-action counts", () => {
    const s = getOrderInboxSummary();
    expect(s.totalLifetime).toBe(47);
    expect(s.needAction).toBe(4);
  });
});

describe("seller orders — order detail #1042", () => {
  it("returns null for unknown order ids", () => {
    expect(getOrderDetailFull("9999")).toBeNull();
  });

  it("returns the Sasha Leblanc partial-fulfillment detail for 1042", () => {
    const d = getOrderDetailFull("1042");
    expect(d).not.toBeNull();
    if (!d) return;
    expect(d.id).toBe("#1042");
    expect(d.status).toBe("Partially fulfilled");
    expect(d.customer.name).toBe("Sasha Leblanc");
    expect(d.fulfillments).toHaveLength(2);
    expect(d.fulfillments[0].status).toBe("shipped");
    expect(d.fulfillments[0].productName).toBe("Persimmon vase");
    expect(d.fulfillments[1].status).toBe("awaiting");
    expect(d.fulfillments[1].productName).toBe("Ash budstem");
    expect(d.refund.total).toBe(30);
    expect(d.refund.lastFour).toBe("4421");
    expect(d.summary.paid).toBe(152);
    expect(d.summary.fee).toBe(6.08);
    expect(d.summary.labelCost).toBe(9.84);
    expect(d.summary.net).toBe(136.08);
    expect(d.timeline).toHaveLength(5);
    expect(d.customerTags).toEqual(["VIP", "Repeat buyer", "Gift"]);
  });
});

describe("seller orders — shipping options", () => {
  it("returns 3 shipping options with exactly one selected", () => {
    const opts = getShippingOptions();
    expect(opts).toHaveLength(3);
    const selected = opts.filter((o) => o.selected);
    expect(selected).toHaveLength(1);
  });

  it("all shipping options have positive prices", () => {
    for (const o of getShippingOptions()) {
      expect(o.price).toBeGreaterThan(0);
    }
  });
});

describe("seller orders — legacy order detail (#1001)", () => {
  it("returns null for an unknown order id", () => {
    expect(getOrderDetail("9999")).toBeNull();
  });

  it("returns Sasha Leblanc detail for order 1001", () => {
    const d = getOrderDetail("1001");
    expect(d).not.toBeNull();
    if (!d) return;
    expect(d.id).toBe("#1001");
    expect(d.customer).toBe("Sasha Leblanc");
    expect(d.subtotal).toBe(86);
    expect(d.net).toBeGreaterThan(0);
    expect(d.feePct).toBe(4);
  });
});
