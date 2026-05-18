import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { PromosTabs } from "@/components/seller/promos/promos-tabs";
import type { PromoTab } from "@/lib/seller/promos/types";

const TABS: PromoTab[] = [
  { label: "Active", count: 3, on: true },
  { label: "Ended", count: 5, on: false },
  { label: "Draft", count: 1, on: false },
];

describe("PromosTabs", () => {
  it("renders a button for each tab", () => {
    render(<PromosTabs tabs={TABS} />);
    expect(screen.getByRole("button", { name: /Active/ })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /Ended/ })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /Draft/ })).toBeInTheDocument();
  });

  it("renders the count badge for each tab", () => {
    render(<PromosTabs tabs={TABS} />);
    expect(screen.getByText("3")).toBeInTheDocument();
    expect(screen.getByText("5")).toBeInTheDocument();
    expect(screen.getByText("1")).toBeInTheDocument();
  });

  it("active tab has dark border class", () => {
    render(<PromosTabs tabs={TABS} />);
    const activeBtn = screen.getByRole("button", { name: /Active/ });
    expect(activeBtn.className).toContain("border-[#1d1d1f]");
  });

  it("inactive tabs have transparent border", () => {
    render(<PromosTabs tabs={TABS} />);
    const endedBtn = screen.getByRole("button", { name: /Ended/ });
    expect(endedBtn.className).toContain("border-transparent");
  });

  it("renders empty list without errors", () => {
    render(<PromosTabs tabs={[]} />);
    expect(screen.queryAllByRole("button")).toHaveLength(0);
  });

  it("renders tab with on=false as default when on is omitted", () => {
    const tabs: PromoTab[] = [{ label: "All", count: 10 }];
    render(<PromosTabs tabs={tabs} />);
    const btn = screen.getByRole("button", { name: /All/ });
    expect(btn.className).toContain("border-transparent");
  });
});
