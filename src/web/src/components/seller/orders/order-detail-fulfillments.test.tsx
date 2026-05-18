import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import type { OrderFulfillment } from "@/lib/seller/orders/types";
import { OrderDetailFulfillments } from "./order-detail-fulfillments";

const SHIPPED: OrderFulfillment = {
  idx: 1,
  of: 2,
  status: "shipped",
  productName: "Clay Vase",
  productSubtitle: "Large · Clay",
  productTone: "clay",
  qty: 1,
  price: 89,
  tracking: "9400111899223450149097",
};

const AWAITING: OrderFulfillment = {
  idx: 2,
  of: 2,
  status: "awaiting",
  productName: "Rust Bowl",
  productSubtitle: "Medium · Rust",
  productTone: "rust",
  qty: 1,
  price: 63,
  restockNote: "restock ETA 3d",
};

describe("OrderDetailFulfillments", () => {
  it("renders product name for each fulfillment", () => {
    render(<OrderDetailFulfillments fulfillments={[SHIPPED, AWAITING]} />);
    expect(screen.getByText("Clay Vase")).toBeInTheDocument();
    expect(screen.getByText("Rust Bowl")).toBeInTheDocument();
  });

  it("renders fulfillment index and total", () => {
    render(<OrderDetailFulfillments fulfillments={[SHIPPED]} />);
    expect(screen.getByText(/Fulfillment 1 of 2/)).toBeInTheDocument();
  });

  it("shows 'shipped' label for shipped fulfillment", () => {
    render(<OrderDetailFulfillments fulfillments={[SHIPPED]} />);
    expect(screen.getByText(/shipped/)).toBeInTheDocument();
  });

  it("shows 'awaiting restock' label for awaiting fulfillment", () => {
    render(<OrderDetailFulfillments fulfillments={[AWAITING]} />);
    expect(screen.getByText(/awaiting restock/)).toBeInTheDocument();
  });

  it("shows tracking number for shipped fulfillment with tracking", () => {
    render(<OrderDetailFulfillments fulfillments={[SHIPPED]} />);
    expect(screen.getByText("9400111899223450149097")).toBeInTheDocument();
  });

  it("shows Buy label button when not shipped", () => {
    render(<OrderDetailFulfillments fulfillments={[AWAITING]} />);
    expect(
      screen.getByRole("button", { name: "Buy label" }),
    ).toBeInTheDocument();
  });

  it("does not show Buy label button for shipped fulfillment", () => {
    render(<OrderDetailFulfillments fulfillments={[SHIPPED]} />);
    expect(
      screen.queryByRole("button", { name: "Buy label" }),
    ).not.toBeInTheDocument();
  });

  it("renders formatted price via money()", () => {
    render(<OrderDetailFulfillments fulfillments={[SHIPPED]} />);
    expect(screen.getByText("$89.00")).toBeInTheDocument();
  });

  it("renders product subtitle", () => {
    render(<OrderDetailFulfillments fulfillments={[SHIPPED]} />);
    expect(screen.getByText("Large · Clay")).toBeInTheDocument();
  });

  it("renders restock note badge when present", () => {
    render(<OrderDetailFulfillments fulfillments={[AWAITING]} />);
    expect(screen.getByText("restock ETA 3d")).toBeInTheDocument();
  });

  it("does not render restock badge when absent", () => {
    render(<OrderDetailFulfillments fulfillments={[SHIPPED]} />);
    expect(screen.queryByText(/restock/)).not.toBeInTheDocument();
  });

  it("renders multiple fulfillments", () => {
    render(<OrderDetailFulfillments fulfillments={[SHIPPED, AWAITING]} />);
    expect(screen.getByText("$89.00")).toBeInTheDocument();
    expect(screen.getByText("$63.00")).toBeInTheDocument();
  });

  it("renders shipped fulfillment without tracking as Buy label button", () => {
    const shippedNoTracking: OrderFulfillment = {
      ...SHIPPED,
      tracking: undefined,
    };
    render(<OrderDetailFulfillments fulfillments={[shippedNoTracking]} />);
    expect(
      screen.getByRole("button", { name: "Buy label" }),
    ).toBeInTheDocument();
  });
});
