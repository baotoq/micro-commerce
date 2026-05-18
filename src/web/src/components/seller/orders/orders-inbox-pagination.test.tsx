import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { OrdersInboxPagination } from "./orders-inbox-pagination";

describe("OrdersInboxPagination", () => {
  it("renders showing summary text", () => {
    render(<OrdersInboxPagination showing="1–10" total={48} />);
    expect(screen.getByText("Showing 1–10 of 48")).toBeInTheDocument();
  });

  it("renders previous and next page buttons", () => {
    render(<OrdersInboxPagination showing="1–10" total={48} />);
    expect(
      screen.getByRole("button", { name: "Previous page" }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Next page" }),
    ).toBeInTheDocument();
  });

  it("renders 5 numbered page buttons", () => {
    render(<OrdersInboxPagination showing="1–10" total={48} />);
    for (let i = 1; i <= 5; i++) {
      expect(
        screen.getByRole("button", { name: `Page ${i}` }),
      ).toBeInTheDocument();
    }
  });

  it("marks page 1 as current page with aria-current", () => {
    render(<OrdersInboxPagination showing="1–10" total={48} />);
    expect(screen.getByRole("button", { name: "Page 1" })).toHaveAttribute(
      "aria-current",
      "page",
    );
  });

  it("does not mark other pages as current", () => {
    render(<OrdersInboxPagination showing="1–10" total={48} />);
    for (let i = 2; i <= 5; i++) {
      expect(
        screen.getByRole("button", { name: `Page ${i}` }),
      ).not.toHaveAttribute("aria-current");
    }
  });

  it("renders correct total in different configurations", () => {
    render(<OrdersInboxPagination showing="11–20" total={200} />);
    expect(screen.getByText("Showing 11–20 of 200")).toBeInTheDocument();
  });
});
