import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import type { OrderRefund } from "@/lib/seller/orders/types";
import { OrderDetailRefund } from "./order-detail-refund";

const BASE_REFUND: OrderRefund = {
  refundable: 152,
  itemsCount: 2,
  items: [
    {
      name: "Clay Vase",
      qty: 1,
      price: 89,
      selected: true,
      partial: 44.5,
    },
    {
      name: "Rust Bowl",
      qty: 1,
      price: 63,
      selected: false,
    },
  ],
  reason: "Item not as described",
  restockOptions: ["Yes", "No"],
  restockSelected: "Yes",
  total: 44.5,
  lastFour: "4242",
};

describe("OrderDetailRefund", () => {
  it("renders 'Issue refund' heading", () => {
    render(<OrderDetailRefund refund={BASE_REFUND} />);
    expect(screen.getAllByText("Issue refund").length).toBeGreaterThan(0);
  });

  it("renders refundable amount via money()", () => {
    render(<OrderDetailRefund refund={BASE_REFUND} />);
    expect(screen.getByText(/refundable: \$152\.00/)).toBeInTheDocument();
  });

  it("renders items count", () => {
    render(<OrderDetailRefund refund={BASE_REFUND} />);
    expect(screen.getByText(/across 2 items/)).toBeInTheDocument();
  });

  it("renders each item name", () => {
    render(<OrderDetailRefund refund={BASE_REFUND} />);
    expect(screen.getByText("Clay Vase")).toBeInTheDocument();
    expect(screen.getByText("Rust Bowl")).toBeInTheDocument();
  });

  it("renders item qty and price via money()", () => {
    render(<OrderDetailRefund refund={BASE_REFUND} />);
    expect(screen.getByText(/qty 1 · paid \$89\.00/)).toBeInTheDocument();
    expect(screen.getByText(/qty 1 · paid \$63\.00/)).toBeInTheDocument();
  });

  it("renders partial refund amount for selected item with partial", () => {
    render(<OrderDetailRefund refund={BASE_REFUND} />);
    expect(screen.getByText("44.50")).toBeInTheDocument();
  });

  it("renders dash for unselected item", () => {
    render(<OrderDetailRefund refund={BASE_REFUND} />);
    expect(screen.getByText("—")).toBeInTheDocument();
  });

  it("renders reason dropdown with correct value", () => {
    render(<OrderDetailRefund refund={BASE_REFUND} />);
    expect(screen.getByText("Item not as described")).toBeInTheDocument();
  });

  it("renders restock options", () => {
    render(<OrderDetailRefund refund={BASE_REFUND} />);
    expect(screen.getByText("Yes")).toBeInTheDocument();
    expect(screen.getByText("No")).toBeInTheDocument();
  });

  it("renders refund total via money()", () => {
    render(<OrderDetailRefund refund={BASE_REFUND} />);
    expect(screen.getByText("$44.50")).toBeInTheDocument();
  });

  it("renders last four digits of card", () => {
    render(<OrderDetailRefund refund={BASE_REFUND} />);
    expect(screen.getByText(/4242/)).toBeInTheDocument();
  });

  it("renders Issue refund button", () => {
    render(<OrderDetailRefund refund={BASE_REFUND} />);
    expect(
      screen.getByRole("button", { name: "Issue refund" }),
    ).toBeInTheDocument();
  });

  it("shows refund label area when selected item has no partial", () => {
    const refund: OrderRefund = {
      ...BASE_REFUND,
      items: [{ name: "Clay Vase", qty: 1, price: 89, selected: true }],
    };
    render(<OrderDetailRefund refund={refund} />);
    expect(screen.getByText("—")).toBeInTheDocument();
  });

  it("renders of price next to partial input", () => {
    render(<OrderDetailRefund refund={BASE_REFUND} />);
    expect(screen.getAllByText(/of \$89\.00/).length).toBeGreaterThan(0);
  });
});
