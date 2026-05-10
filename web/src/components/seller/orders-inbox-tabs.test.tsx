import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import type { OrderInboxTab } from "@/lib/seller/types";
import { OrdersInboxTabs } from "./orders-inbox-tabs";

const TABS: OrderInboxTab[] = [
  { label: "All orders", count: 24, on: true },
  { label: "New", count: 3 },
  { label: "Packed", count: 7 },
];

describe("OrdersInboxTabs", () => {
  it("renders a tablist with one tab per entry", () => {
    render(<OrdersInboxTabs tabs={TABS} />);
    expect(screen.getByRole("tablist")).toBeInTheDocument();
    expect(screen.getAllByRole("tab")).toHaveLength(3);
  });

  it("renders each tab label", () => {
    render(<OrdersInboxTabs tabs={TABS} />);
    expect(screen.getByText("All orders")).toBeInTheDocument();
    expect(screen.getByText("New")).toBeInTheDocument();
    expect(screen.getByText("Packed")).toBeInTheDocument();
  });

  it("renders each tab count badge", () => {
    render(<OrdersInboxTabs tabs={TABS} />);
    expect(screen.getByText("24")).toBeInTheDocument();
    expect(screen.getByText("3")).toBeInTheDocument();
    expect(screen.getByText("7")).toBeInTheDocument();
  });

  it("sets aria-selected=true on the active tab", () => {
    render(<OrdersInboxTabs tabs={TABS} />);
    const tabs = screen.getAllByRole("tab");
    expect(tabs[0]).toHaveAttribute("aria-selected", "true");
    expect(tabs[1]).toHaveAttribute("aria-selected", "false");
    expect(tabs[2]).toHaveAttribute("aria-selected", "false");
  });

  it("sets aria-selected=false on all tabs when none is active", () => {
    const noActiveTabs: OrderInboxTab[] = [
      { label: "All orders", count: 5 },
      { label: "New", count: 2 },
    ];
    render(<OrdersInboxTabs tabs={noActiveTabs} />);
    for (const tab of screen.getAllByRole("tab")) {
      expect(tab).toHaveAttribute("aria-selected", "false");
    }
  });
});
