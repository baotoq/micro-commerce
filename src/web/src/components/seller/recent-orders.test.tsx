// web/src/components/seller/recent-orders.test.tsx
import { render, screen, within } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { RecentOrders } from "@/components/seller/recent-orders";

describe("RecentOrders", () => {
  const orders = [
    {
      id: "#1042",
      customer: "Mia Chen",
      items: 2,
      total: 172,
      status: "paid" as const,
      placedAt: "2026-04-08",
    },
    {
      id: "#1041",
      customer: "Theo Park",
      items: 1,
      total: 86,
      status: "fulfilled" as const,
      placedAt: "2026-04-07",
    },
    {
      id: "#1040",
      customer: "Sara Novak",
      items: 3,
      total: 248,
      status: "refunded" as const,
      placedAt: "2026-04-07",
    },
  ];

  it('renders a "Recent orders" heading', () => {
    render(<RecentOrders orders={orders} />);
    expect(
      screen.getByRole("heading", { name: "Recent orders" }),
    ).toBeInTheDocument();
  });

  it("renders column headers: Order, Customer, Items, Total, Status", () => {
    render(<RecentOrders orders={orders} />);
    expect(screen.getByText("Order")).toBeInTheDocument();
    expect(screen.getByText("Customer")).toBeInTheDocument();
    expect(screen.getByText("Items")).toBeInTheDocument();
    expect(screen.getByText("Total")).toBeInTheDocument();
    expect(screen.getByText("Status")).toBeInTheDocument();
  });

  it("renders one row per order", () => {
    render(<RecentOrders orders={orders} />);
    const tbody = screen.getAllByRole("rowgroup")[1];
    const rows = within(tbody).getAllByRole("row");
    expect(rows).toHaveLength(3);
  });

  it("renders each order id", () => {
    render(<RecentOrders orders={orders} />);
    expect(screen.getByText("#1042")).toBeInTheDocument();
    expect(screen.getByText("#1041")).toBeInTheDocument();
    expect(screen.getByText("#1040")).toBeInTheDocument();
  });

  it("renders customer names", () => {
    render(<RecentOrders orders={orders} />);
    expect(screen.getByText("Mia Chen")).toBeInTheDocument();
    expect(screen.getByText("Theo Park")).toBeInTheDocument();
    expect(screen.getByText("Sara Novak")).toBeInTheDocument();
  });

  it("renders formatted totals", () => {
    render(<RecentOrders orders={orders} />);
    expect(screen.getByText("$172.00")).toBeInTheDocument();
    expect(screen.getByText("$86.00")).toBeInTheDocument();
    expect(screen.getByText("$248.00")).toBeInTheDocument();
  });

  it("renders status values", () => {
    render(<RecentOrders orders={orders} />);
    expect(screen.getByText("paid")).toBeInTheDocument();
    expect(screen.getByText("fulfilled")).toBeInTheDocument();
    expect(screen.getByText("refunded")).toBeInTheDocument();
  });

  it("renders an empty table body when no orders are provided", () => {
    render(<RecentOrders orders={[]} />);
    const tbody = screen.getAllByRole("rowgroup")[1];
    expect(within(tbody).queryAllByRole("row")).toHaveLength(0);
  });
});
