import { render, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

const readCart = vi.fn();
vi.mock("@/lib/storefront/cart-cookie", () => ({
  readCart: (...a: unknown[]) => readCart(...a),
}));

const fetchProductBySku = vi.fn();
vi.mock("@/lib/catalog/api", () => ({
  fetchProductBySku: (...a: unknown[]) => fetchProductBySku(...a),
}));

const fetchPromotionByCode = vi.fn();
vi.mock("@/lib/catalog/promotions", () => ({
  fetchPromotionByCode: (...a: unknown[]) => fetchPromotionByCode(...a),
}));

// The cart line + promo widgets are client components; stub them so the server
// page can be rendered in jsdom and we can assert which lines survive filtering.
vi.mock("@/components/storefront/cart-line-row", () => ({
  CartLineRow: ({
    product,
    qty,
  }: {
    product: { sku: string };
    qty: number;
  }) => <div data-testid="cart-line" data-sku={product.sku} data-qty={qty} />,
}));
vi.mock("@/components/storefront/promo-form", () => ({
  PromoForm: () => <div data-testid="promo-form" />,
}));

import CartPage from "@/app/(storefront)/cart/page";

const vase = {
  sku: "MC-VS-001",
  name: "Persimmon vase",
  category: "Vessels",
  price: 86,
  inventory: 24,
  status: "active",
  photoUrls: [],
};
const bowl = {
  sku: "MC-BW-014",
  name: "Forest bowl",
  category: "Tableware",
  price: 68,
  inventory: 10,
  status: "active",
  photoUrls: [],
};

beforeEach(() => {
  vi.clearAllMocks();
  fetchPromotionByCode.mockResolvedValue(null);
});

describe("cart page", () => {
  it("shows the empty state with a shop CTA when the cart has no lines", async () => {
    readCart.mockResolvedValue({ lines: [] });
    render(await CartPage());
    expect(screen.getByText(/is empty/i)).toBeInTheDocument();
    expect(
      screen.getByRole("link", { name: /Shop the collection/ }),
    ).toBeInTheDocument();
    expect(screen.queryByTestId("cart-line")).not.toBeInTheDocument();
  });

  it("renders a row per buyable line and totals only those lines", async () => {
    readCart.mockResolvedValue({
      lines: [
        { sku: "MC-VS-001", qty: 1 },
        { sku: "MC-BW-014", qty: 2 },
      ],
    });
    fetchProductBySku.mockImplementation((sku: string) =>
      Promise.resolve(sku === "MC-VS-001" ? vase : bowl),
    );

    render(await CartPage());

    expect(screen.getAllByTestId("cart-line")).toHaveLength(2);
    expect(screen.getByText("Subtotal · 2 items")).toBeInTheDocument();
    // 86 + 2*68 = 222
    expect(screen.getByText("$222.00")).toBeInTheDocument();
    expect(screen.queryByText(/no longer available/)).not.toBeInTheDocument();
  });

  it("warns about and drops a stale line whose product vanished", async () => {
    readCart.mockResolvedValue({
      lines: [
        { sku: "MC-VS-001", qty: 1 },
        { sku: "MC-GONE", qty: 3 },
      ],
    });
    fetchProductBySku.mockImplementation((sku: string) =>
      Promise.resolve(sku === "MC-VS-001" ? vase : null),
    );

    render(await CartPage());

    // Only the surviving vase line renders.
    expect(screen.getAllByTestId("cart-line")).toHaveLength(1);
    expect(
      screen.getByText(/1 item is no longer available/),
    ).toBeInTheDocument();
    // The vanished line is excluded from the priced subtotal (just the $86 vase).
    expect(screen.getByText("Subtotal · 1 items")).toBeInTheDocument();
    expect(screen.getByText("$86.00")).toBeInTheDocument();
  });

  it("drops an inactive (draft) line and uses the plural warning copy", async () => {
    readCart.mockResolvedValue({
      lines: [
        { sku: "MC-DR-001", qty: 1 },
        { sku: "MC-GONE", qty: 1 },
      ],
    });
    fetchProductBySku.mockImplementation((sku: string) =>
      Promise.resolve(
        sku === "MC-DR-001"
          ? { ...vase, sku: "MC-DR-001", status: "draft" }
          : null,
      ),
    );

    render(await CartPage());

    expect(screen.queryByTestId("cart-line")).not.toBeInTheDocument();
    expect(
      screen.getByText(/2 items are no longer available/),
    ).toBeInTheDocument();
  });
});
