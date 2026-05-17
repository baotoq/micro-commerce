import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { AnalyticsKpiRow } from "@/components/seller/analytics-kpi-row";
import type { KpiPoint } from "@/lib/seller/types";

describe("AnalyticsKpiRow", () => {
  const kpis: KpiPoint[] = [
    { label: "Revenue", value: 9800, format: "currency", delta: 8 },
    { label: "Orders", value: 124, format: "number", delta: -3 },
    { label: "Conversion", value: 2.4, format: "percent" },
    { label: "AOV", value: 79.03, format: "currency", delta: 5 },
  ];

  it("renders a card for each kpi", () => {
    render(<AnalyticsKpiRow kpis={kpis} />);
    expect(screen.getByText("Revenue")).toBeInTheDocument();
    expect(screen.getByText("Orders")).toBeInTheDocument();
    expect(screen.getByText("Conversion")).toBeInTheDocument();
    expect(screen.getByText("AOV")).toBeInTheDocument();
  });

  it("renders formatted values for each kpi", () => {
    render(<AnalyticsKpiRow kpis={kpis} />);
    expect(screen.getByText("$9,800.00")).toBeInTheDocument();
    expect(screen.getByText("124")).toBeInTheDocument();
    expect(screen.getByText("2.4%")).toBeInTheDocument();
    expect(screen.getByText("$79.03")).toBeInTheDocument();
  });

  it("renders an empty grid with no kpis", () => {
    const { container } = render(<AnalyticsKpiRow kpis={[]} />);
    const grid = container.firstChild as HTMLElement;
    expect(grid).toBeInTheDocument();
    expect(grid.children).toHaveLength(0);
  });

  it("renders a single kpi without error", () => {
    const single: KpiPoint[] = [
      { label: "Visitors", value: 500, format: "number" },
    ];
    render(<AnalyticsKpiRow kpis={single} />);
    expect(screen.getByText("Visitors")).toBeInTheDocument();
  });
});
