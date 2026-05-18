// web/src/components/seller/top-products.test.tsx
import { render, screen, within } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { TopProducts } from "@/components/seller/analytics/top-products";

describe("TopProducts", () => {
  const products = [
    { sku: "MC-VS-001", name: "Persimmon vase", units: 48, revenue: 4128 },
    { sku: "MC-CR-003", name: "Indigo carafe", units: 32, revenue: 3520 },
    { sku: "MC-VS-002", name: "Shadow vase, tall", units: 22, revenue: 2728 },
  ];

  it('renders a "Top products" heading', () => {
    render(<TopProducts products={products} />);
    expect(
      screen.getByRole("heading", { name: "Top products" }),
    ).toBeInTheDocument();
  });

  it("renders one list item per product", () => {
    render(<TopProducts products={products} />);
    const list = screen.getByRole("list");
    const items = within(list).getAllByRole("listitem");
    expect(items).toHaveLength(3);
  });

  it("renders each product name", () => {
    render(<TopProducts products={products} />);
    expect(screen.getByText("Persimmon vase")).toBeInTheDocument();
    expect(screen.getByText("Indigo carafe")).toBeInTheDocument();
    expect(screen.getByText("Shadow vase, tall")).toBeInTheDocument();
  });

  it("renders units and formatted revenue for each product", () => {
    render(<TopProducts products={products} />);
    expect(screen.getByText("48 · $4,128.00")).toBeInTheDocument();
    expect(screen.getByText("32 · $3,520.00")).toBeInTheDocument();
    expect(screen.getByText("22 · $2,728.00")).toBeInTheDocument();
  });

  it("renders product list in order", () => {
    render(<TopProducts products={products} />);
    const list = screen.getByRole("list");
    const items = within(list).getAllByRole("listitem");
    expect(items[0]).toHaveTextContent("Persimmon vase");
    expect(items[2]).toHaveTextContent("Shadow vase, tall");
  });

  it("renders an empty list when no products are given", () => {
    render(<TopProducts products={[]} />);
    const list = screen.getByRole("list");
    expect(within(list).queryAllByRole("listitem")).toHaveLength(0);
  });

  it("renders a single product without error", () => {
    const single = [
      { sku: "MC-VS-001", name: "Only product", units: 5, revenue: 500 },
    ];
    render(<TopProducts products={single} />);
    expect(screen.getByText("Only product")).toBeInTheDocument();
    expect(screen.getByText("5 · $500.00")).toBeInTheDocument();
  });
});
