import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { PromoStatCard } from "@/components/seller/promos/promo-stat-card";
import type { PromoStat } from "@/lib/seller/promos/types";

// New stat shape: Active / Draft / Ended / Total (from §4.2 of design spec)
const STAT: PromoStat = {
  label: "Active",
  value: "3",
  sub: "currently live",
  spark: [10, 20, 15, 30, 25, 40],
};

describe("PromoStatCard", () => {
  it("renders the stat label", () => {
    render(<PromoStatCard stat={STAT} />);
    expect(screen.getByText("Active")).toBeInTheDocument();
  });

  it("renders the stat value", () => {
    render(<PromoStatCard stat={STAT} />);
    expect(screen.getByText("3")).toBeInTheDocument();
  });

  it("renders the sub text", () => {
    render(<PromoStatCard stat={STAT} />);
    expect(screen.getByText("currently live")).toBeInTheDocument();
  });

  it("renders the sparkline chart with a meaningful aria-label derived from the stat label", () => {
    render(<PromoStatCard stat={STAT} />);
    expect(
      screen.getByRole("img", { name: /Active trend/i }),
    ).toBeInTheDocument();
  });

  it("renders Draft stat card", () => {
    const draftStat: PromoStat = {
      label: "Draft",
      value: "1",
      sub: "not yet started",
      spark: [0, 0, 1],
    };
    render(<PromoStatCard stat={draftStat} />);
    expect(screen.getByText("Draft")).toBeInTheDocument();
    expect(screen.getByText("not yet started")).toBeInTheDocument();
  });

  it("renders Total stat card", () => {
    const totalStat: PromoStat = {
      label: "Total",
      value: "5",
      sub: "all-time",
      spark: [],
    };
    render(<PromoStatCard stat={totalStat} />);
    expect(screen.getByText("Total")).toBeInTheDocument();
    expect(screen.getByText("all-time")).toBeInTheDocument();
  });

  it("renders with empty spark array without error", () => {
    const emptySpark: PromoStat = {
      label: "Ended",
      value: "0",
      sub: "past their window",
      spark: [],
    };
    render(<PromoStatCard stat={emptySpark} />);
    expect(screen.getByText("Ended")).toBeInTheDocument();
  });
});
