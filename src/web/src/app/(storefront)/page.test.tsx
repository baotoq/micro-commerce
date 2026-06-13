import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

vi.mock("@/lib/storefront/data", () => ({
  SHOP_PAGE_SIZE: 12,
  getShopProducts: vi.fn().mockResolvedValue({
    items: [
      {
        sku: "MC-VS-001",
        name: "Persimmon vase",
        category: "Vessels",
        price: 86,
        inventory: 24,
        status: "active",
        views7d: 0,
        description: null,
        tags: [],
        weight: 0.1,
        origin: "Oakland, CA",
        photoUrls: [],
      },
    ],
    total: 42,
    page: 1,
    pageSize: 12,
  }),
  getShopCategories: vi
    .fn()
    .mockResolvedValue(["Drinkware", "Tableware", "Vessels"]),
}));

import ShopHome from "@/app/(storefront)/page";

describe("storefront home", () => {
  it("renders the grid, count, and category chips from real data", async () => {
    render(await ShopHome({ searchParams: Promise.resolve({}) }));
    expect(screen.getByText("Persimmon vase")).toBeInTheDocument();
    expect(screen.getByText("$86.00")).toBeInTheDocument();
    expect(screen.getByText("42 pieces")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Vessels" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "All" })).toBeInTheDocument();
  });
});
