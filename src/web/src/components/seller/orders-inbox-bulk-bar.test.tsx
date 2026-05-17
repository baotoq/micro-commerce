import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { OrdersInboxBulkBar } from "./orders-inbox-bulk-bar";

describe("OrdersInboxBulkBar", () => {
  it("renders selected count and total", () => {
    render(<OrdersInboxBulkBar selectedCount={3} total="24" />);
    expect(screen.getByText("3 orders selected")).toBeInTheDocument();
    expect(screen.getByText("— 24 total")).toBeInTheDocument();
  });

  it("renders Print labels button", () => {
    render(<OrdersInboxBulkBar selectedCount={1} total="10" />);
    expect(
      screen.getByRole("button", { name: "Print labels" }),
    ).toBeInTheDocument();
  });

  it("renders Mark packed button", () => {
    render(<OrdersInboxBulkBar selectedCount={1} total="10" />);
    expect(
      screen.getByRole("button", { name: "Mark packed" }),
    ).toBeInTheDocument();
  });

  it("renders Bulk message button", () => {
    render(<OrdersInboxBulkBar selectedCount={1} total="10" />);
    expect(
      screen.getByRole("button", { name: "Bulk message" }),
    ).toBeInTheDocument();
  });

  it("renders Cancel button", () => {
    render(<OrdersInboxBulkBar selectedCount={1} total="10" />);
    expect(screen.getByRole("button", { name: "Cancel" })).toBeInTheDocument();
  });

  it("updates selected count display when prop changes", () => {
    const { rerender } = render(
      <OrdersInboxBulkBar selectedCount={5} total="24" />,
    );
    expect(screen.getByText("5 orders selected")).toBeInTheDocument();
    rerender(<OrdersInboxBulkBar selectedCount={10} total="24" />);
    expect(screen.getByText("10 orders selected")).toBeInTheDocument();
  });
});
