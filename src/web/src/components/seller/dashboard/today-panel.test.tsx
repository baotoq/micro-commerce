// web/src/components/seller/today-panel.test.tsx
import { render, screen, within } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { TodayPanel } from "@/components/seller/dashboard/today-panel";

describe("TodayPanel", () => {
  const items = [
    { label: "Pack 2 orders ready to ship" },
    { label: "Low inventory alert: Ember tea bowl (2 left)" },
    { label: "3 new reviews to moderate" },
  ];

  it('renders a "Today" heading', () => {
    render(<TodayPanel items={items} />);
    expect(screen.getByRole("heading", { name: "Today" })).toBeInTheDocument();
  });

  it("renders one list item per entry", () => {
    render(<TodayPanel items={items} />);
    const list = screen.getByRole("list");
    const listItems = within(list).getAllByRole("listitem");
    expect(listItems).toHaveLength(3);
  });

  it("renders each item label as text", () => {
    render(<TodayPanel items={items} />);
    for (const item of items) {
      expect(screen.getByText(item.label)).toBeInTheDocument();
    }
  });

  it("preserves order of items", () => {
    render(<TodayPanel items={items} />);
    const list = screen.getByRole("list");
    const listItems = within(list).getAllByRole("listitem");
    expect(listItems[0]).toHaveTextContent("Pack 2 orders ready to ship");
    expect(listItems[2]).toHaveTextContent("3 new reviews to moderate");
  });

  it("renders an empty list when no items are provided", () => {
    render(<TodayPanel items={[]} />);
    const list = screen.getByRole("list");
    expect(within(list).queryAllByRole("listitem")).toHaveLength(0);
  });

  it("renders a single item without error", () => {
    render(<TodayPanel items={[{ label: "One task" }]} />);
    expect(screen.getByText("One task")).toBeInTheDocument();
  });
});
