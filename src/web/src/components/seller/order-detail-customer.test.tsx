import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import type { OrderCustomer } from "@/lib/seller/types";
import { OrderDetailCustomer } from "./order-detail-customer";

const CUSTOMER: OrderCustomer = {
  name: "Jane Smith",
  shortName: "Jane Smith",
  lifetimeOrdersLabel: "3 orders · $420.00",
  ship: { line1: "123 Main St", line2: "Portland, OR 97201" },
  billSameAsShip: true,
  email: "jane@example.com",
};

describe("OrderDetailCustomer", () => {
  it("renders customer name", () => {
    render(<OrderDetailCustomer customer={CUSTOMER} />);
    expect(screen.getByText("Jane Smith")).toBeInTheDocument();
  });

  it("renders initials from shortName", () => {
    render(<OrderDetailCustomer customer={CUSTOMER} />);
    expect(screen.getByText("JS")).toBeInTheDocument();
  });

  it("renders lifetime orders label", () => {
    render(<OrderDetailCustomer customer={CUSTOMER} />);
    expect(screen.getByText("3 orders · $420.00")).toBeInTheDocument();
  });

  it("renders shipping address lines", () => {
    render(<OrderDetailCustomer customer={CUSTOMER} />);
    const container = screen.getByText("Ship to").nextElementSibling;
    expect(container?.textContent).toContain("123 Main St");
    expect(container?.textContent).toContain("Portland, OR 97201");
  });

  it("renders 'Bill to · same as ship' when billSameAsShip is true", () => {
    render(<OrderDetailCustomer customer={CUSTOMER} />);
    expect(screen.getByText("Bill to · same as ship")).toBeInTheDocument();
  });

  it("does not render bill-same-as-ship note when billSameAsShip is false", () => {
    render(
      <OrderDetailCustomer customer={{ ...CUSTOMER, billSameAsShip: false }} />,
    );
    expect(
      screen.queryByText("Bill to · same as ship"),
    ).not.toBeInTheDocument();
  });

  it("renders email address", () => {
    render(<OrderDetailCustomer customer={CUSTOMER} />);
    expect(screen.getByText("jane@example.com")).toBeInTheDocument();
  });

  it("renders Profile link button", () => {
    render(<OrderDetailCustomer customer={CUSTOMER} />);
    expect(screen.getByText("Profile →")).toBeInTheDocument();
  });

  it("renders single-word shortName as single initial", () => {
    render(
      <OrderDetailCustomer customer={{ ...CUSTOMER, shortName: "Jane" }} />,
    );
    expect(screen.getByText("J")).toBeInTheDocument();
  });
});
