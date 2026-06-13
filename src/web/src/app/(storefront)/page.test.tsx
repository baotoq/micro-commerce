import { render, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

const getShopProducts = vi.fn();
const getShopCategories = vi.fn();
vi.mock("@/lib/storefront/data", () => ({
  SHOP_PAGE_SIZE: 12,
  getShopProducts: (...a: unknown[]) => getShopProducts(...a),
  getShopCategories: (...a: unknown[]) => getShopCategories(...a),
}));

import ShopHome from "@/app/(storefront)/page";

const product = {
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
};

beforeEach(() => {
  vi.clearAllMocks();
  getShopProducts.mockResolvedValue({
    items: [product],
    total: 42,
    page: 1,
    pageSize: 12,
  });
  getShopCategories.mockResolvedValue(["Drinkware", "Tableware", "Vessels"]);
});

function paramsOf(href: string) {
  return new URL(href, "http://localhost").searchParams;
}

describe("storefront home", () => {
  it("renders the grid, count, and category chips from real data", async () => {
    render(await ShopHome({ searchParams: Promise.resolve({}) }));
    expect(screen.getByText("Persimmon vase")).toBeInTheDocument();
    expect(screen.getByText("$86.00")).toBeInTheDocument();
    expect(screen.getByText("42 pieces")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Vessels" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "All" })).toBeInTheDocument();
  });

  it("sort links preserve the active search and category", async () => {
    render(
      await ShopHome({
        searchParams: Promise.resolve({ q: "vase", category: "Vessels" }),
      }),
    );
    const priceUp = screen.getByRole("link", { name: /Price ↑/ });
    const sp = paramsOf(priceUp.getAttribute("href") ?? "");
    expect(sp.get("sort")).toBe("price-asc");
    expect(sp.get("q")).toBe("vase");
    expect(sp.get("category")).toBe("Vessels");
  });

  it("pagination links preserve search, category, and sort", async () => {
    getShopProducts.mockResolvedValue({
      items: [product],
      total: 42,
      page: 1,
      pageSize: 12,
    });
    render(
      await ShopHome({
        searchParams: Promise.resolve({
          q: "vase",
          category: "Vessels",
          sort: "price-desc",
        }),
      }),
    );
    const next = screen.getByRole("link", { name: /Next →/ });
    const sp = paramsOf(next.getAttribute("href") ?? "");
    expect(sp.get("page")).toBe("2");
    expect(sp.get("q")).toBe("vase");
    expect(sp.get("category")).toBe("Vessels");
    expect(sp.get("sort")).toBe("price-desc");
  });

  it("category chip links preserve the active search", async () => {
    render(await ShopHome({ searchParams: Promise.resolve({ q: "vase" }) }));
    const chip = screen.getByRole("link", { name: "Vessels" });
    const sp = paramsOf(chip.getAttribute("href") ?? "");
    expect(sp.get("category")).toBe("Vessels");
    expect(sp.get("q")).toBe("vase");
  });
});
