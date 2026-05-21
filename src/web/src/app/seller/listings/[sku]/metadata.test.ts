// TDD for audit#8: dynamic seller routes (`[sku]/edit`, `[sku]/preview`,
// orders `[id]`) need per-page <title>/<meta> so browser tabs and shared
// links reflect the item. Without `generateMetadata`, all three inherit the
// generic "micro-commerce" title from `app/layout.tsx`.

import { describe, expect, it } from "vitest";
import { generateMetadata as editMetadata } from "@/app/seller/listings/[sku]/edit/page";
import { generateMetadata as previewMetadata } from "@/app/seller/listings/[sku]/preview/page";
import { generateMetadata as orderMetadata } from "@/app/seller/orders/[id]/page";

describe("dynamic seller route metadata", () => {
  it("edit page titles the tab with the SKU", async () => {
    const meta = await editMetadata({
      params: Promise.resolve({ sku: "MC-VS-001" }),
    });
    expect(meta.title).toBe("Edit MC-VS-001 · Micro Commerce");
  });

  it("preview page titles the tab with the SKU", async () => {
    const meta = await previewMetadata({
      params: Promise.resolve({ sku: "MC-VS-001" }),
    });
    expect(meta.title).toBe("Preview MC-VS-001 · Micro Commerce");
  });

  it("order detail page titles the tab with the order id", async () => {
    const meta = await orderMetadata({
      params: Promise.resolve({ id: "ORD-1042" }),
    });
    expect(meta.title).toBe("Order ORD-1042 · Micro Commerce");
  });
});
