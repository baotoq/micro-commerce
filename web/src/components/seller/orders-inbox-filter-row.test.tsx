import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { OrdersInboxFilterRow } from "./orders-inbox-filter-row";

describe("OrdersInboxFilterRow", () => {
  it("renders search placeholder text", () => {
    render(<OrdersInboxFilterRow />);
    expect(
      screen.getByText("Search by order, customer, SKU…"),
    ).toBeInTheDocument();
  });

  it("renders Status filter chip", () => {
    render(<OrdersInboxFilterRow />);
    expect(screen.getByRole("button", { name: /Status/i })).toBeInTheDocument();
  });

  it("renders Ship method filter chip", () => {
    render(<OrdersInboxFilterRow />);
    expect(
      screen.getByRole("button", { name: /Ship method/i }),
    ).toBeInTheDocument();
  });

  it("renders Date filter chip", () => {
    render(<OrdersInboxFilterRow />);
    expect(
      screen.getByRole("button", { name: /Date · last 30d/i }),
    ).toBeInTheDocument();
  });

  it("renders More filters button", () => {
    render(<OrdersInboxFilterRow />);
    expect(
      screen.getByRole("button", { name: /More filters/i }),
    ).toBeInTheDocument();
  });

  it("renders all three filter chips", () => {
    render(<OrdersInboxFilterRow />);
    const filterButtons = screen
      .getAllByRole("button")
      .filter((btn) =>
        ["Status", "Ship method", "Date · last 30d"].some((label) =>
          btn.textContent?.includes(label),
        ),
      );
    expect(filterButtons).toHaveLength(3);
  });
});
