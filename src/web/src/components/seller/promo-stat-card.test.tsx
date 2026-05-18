import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { PromoStatCard } from "@/components/seller/promo-stat-card";
import type { PromoStat } from "@/lib/seller/promos/types";

const STAT: PromoStat = {
  label: "Redemptions",
  value: "142",
  sub: "+12% vs last period",
  spark: [10, 20, 15, 30, 25, 40],
};

describe("PromoStatCard", () => {
  it("renders the stat label", () => {
    render(<PromoStatCard stat={STAT} />);
    expect(screen.getByText("Redemptions")).toBeInTheDocument();
  });

  it("renders the stat value", () => {
    render(<PromoStatCard stat={STAT} />);
    expect(screen.getByText("142")).toBeInTheDocument();
  });

  it("renders the sub text", () => {
    render(<PromoStatCard stat={STAT} />);
    expect(screen.getByText("+12% vs last period")).toBeInTheDocument();
  });

  it("renders the sparkline chart", () => {
    render(<PromoStatCard stat={STAT} />);
    expect(
      screen.getByRole("img", { name: "Sparkline chart" }),
    ).toBeInTheDocument();
  });

  it("renders with dollar value", () => {
    const dollarStat: PromoStat = {
      label: "Revenue",
      value: "$2,740",
      sub: "driven by promos",
      spark: [100, 200, 150],
    };
    render(<PromoStatCard stat={dollarStat} />);
    expect(screen.getByText("$2,740")).toBeInTheDocument();
    expect(screen.getByText("driven by promos")).toBeInTheDocument();
  });

  it("renders with empty spark array without error", () => {
    const emptySpark: PromoStat = {
      label: "Clicks",
      value: "0",
      sub: "no data",
      spark: [],
    };
    render(<PromoStatCard stat={emptySpark} />);
    expect(screen.getByText("Clicks")).toBeInTheDocument();
  });
});
