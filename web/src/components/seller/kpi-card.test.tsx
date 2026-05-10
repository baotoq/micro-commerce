import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { KpiCard } from "@/components/seller/kpi-card";
import type { KpiPoint } from "@/lib/seller/types";

describe("KpiCard", () => {
  it("renders label and currency value", () => {
    const kpi: KpiPoint = {
      label: "Revenue",
      value: 1234.56,
      format: "currency",
    };
    render(<KpiCard kpi={kpi} />);
    expect(screen.getByText("Revenue")).toBeInTheDocument();
    expect(screen.getByText("$1,234.56")).toBeInTheDocument();
  });

  it("renders number format value", () => {
    const kpi: KpiPoint = { label: "Orders", value: 42, format: "number" };
    render(<KpiCard kpi={kpi} />);
    expect(screen.getByText("42")).toBeInTheDocument();
  });

  it("renders percent format value", () => {
    const kpi: KpiPoint = {
      label: "Conversion",
      value: 3.7,
      format: "percent",
    };
    render(<KpiCard kpi={kpi} />);
    expect(screen.getByText("3.7%")).toBeInTheDocument();
  });

  it("shows upward triangle and delta when delta is positive", () => {
    const kpi: KpiPoint = {
      label: "Revenue",
      value: 500,
      format: "currency",
      delta: 12,
    };
    render(<KpiCard kpi={kpi} />);
    expect(screen.getByText(/▲/)).toBeInTheDocument();
    expect(screen.getByText(/12%/)).toBeInTheDocument();
  });

  it("shows downward triangle when delta is negative", () => {
    const kpi: KpiPoint = {
      label: "Revenue",
      value: 500,
      format: "currency",
      delta: -5,
    };
    render(<KpiCard kpi={kpi} />);
    expect(screen.getByText(/▼/)).toBeInTheDocument();
    expect(screen.getByText(/5%/)).toBeInTheDocument();
  });

  it("shows upward triangle when delta is zero", () => {
    const kpi: KpiPoint = {
      label: "Revenue",
      value: 500,
      format: "currency",
      delta: 0,
    };
    render(<KpiCard kpi={kpi} />);
    expect(screen.getByText(/▲/)).toBeInTheDocument();
  });

  it("hides delta row when delta is undefined", () => {
    const kpi: KpiPoint = { label: "Revenue", value: 500, format: "currency" };
    render(<KpiCard kpi={kpi} />);
    expect(screen.queryByText(/▲/)).not.toBeInTheDocument();
    expect(screen.queryByText(/▼/)).not.toBeInTheDocument();
  });
});
