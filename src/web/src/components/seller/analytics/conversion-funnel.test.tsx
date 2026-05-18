// web/src/components/seller/conversion-funnel.test.tsx
import { render, screen, within } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { ConversionFunnel } from "@/components/seller/analytics/conversion-funnel";

describe("ConversionFunnel", () => {
  const stages = [
    { label: "Storefront views", count: 12000 },
    { label: "Product views", count: 8400 },
    { label: "Added to cart", count: 2300 },
    { label: "Checkout started", count: 1100 },
    { label: "Purchased", count: 410 },
  ];

  it("renders one row per stage in order", () => {
    render(<ConversionFunnel stages={stages} />);
    const list = screen.getByRole("list");
    const items = within(list).getAllByRole("listitem");
    expect(items).toHaveLength(5);
    expect(items[0]).toHaveTextContent("Storefront views");
    expect(items[4]).toHaveTextContent("Purchased");
  });
});
