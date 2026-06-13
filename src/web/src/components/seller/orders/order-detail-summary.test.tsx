import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { OrderDetailSummary } from "./order-detail-summary";

const SUMMARY = {
  subtotal: 152,
  itemsCount: 2,
  shipping: 0,
  tax: 0,
  paid: 152,
  feePct: 4,
  fee: 6.08,
  labelCarrier: "USPS",
  labelCost: 9.84,
  net: 136.08,
  discountCode: null,
  discountAmount: 0,
};

describe("OrderDetailSummary", () => {
  it("renders subtotal", () => {
    render(<OrderDetailSummary summary={SUMMARY} />);
    expect(screen.getByText("Subtotal · 2 items")).toBeInTheDocument();
    expect(screen.getAllByText("$152.00").length).toBeGreaterThan(0);
  });

  it("renders shipping", () => {
    render(<OrderDetailSummary summary={SUMMARY} />);
    expect(screen.getByText("Shipping")).toBeInTheDocument();
    expect(screen.getAllByText("$0.00").length).toBeGreaterThan(0);
  });

  it("renders tax", () => {
    render(<OrderDetailSummary summary={SUMMARY} />);
    expect(screen.getByText("Tax")).toBeInTheDocument();
  });

  it("renders Customer paid", () => {
    render(<OrderDetailSummary summary={SUMMARY} />);
    expect(screen.getByText("Customer paid")).toBeInTheDocument();
  });

  it("renders Micro fee", () => {
    render(<OrderDetailSummary summary={SUMMARY} />);
    expect(screen.getByText("Micro fee · 4%")).toBeInTheDocument();
    expect(screen.getByText("−$6.08")).toBeInTheDocument();
  });

  it("renders label cost", () => {
    render(<OrderDetailSummary summary={SUMMARY} />);
    expect(screen.getByText("Shipping label · USPS")).toBeInTheDocument();
    expect(screen.getByText("−$9.84")).toBeInTheDocument();
  });

  it("renders net You'll receive", () => {
    render(<OrderDetailSummary summary={SUMMARY} />);
    expect(screen.getByText("You'll receive")).toBeInTheDocument();
    expect(screen.getByText("$136.08")).toBeInTheDocument();
  });

  it("renders the promo line when the order carried a discount", () => {
    render(
      <OrderDetailSummary
        summary={{ ...SUMMARY, discountCode: "WELCOME10", discountAmount: 10 }}
      />,
    );
    expect(screen.getByText("Promo · WELCOME10")).toBeInTheDocument();
    expect(screen.getByText("−$10.00")).toBeInTheDocument();
  });

  it("omits the promo line when there is no discount", () => {
    render(
      <OrderDetailSummary
        summary={{ ...SUMMARY, discountCode: null, discountAmount: 0 }}
      />,
    );
    expect(screen.queryByText(/Promo ·/)).not.toBeInTheDocument();
  });
});
