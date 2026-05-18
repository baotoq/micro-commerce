import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { RevenueChart } from "@/components/seller/revenue-chart";
import type { RevenuePoint } from "@/lib/seller/analytics/types";

describe("RevenueChart", () => {
  const points: RevenuePoint[] = [
    { day: "Mon", amount: 400 },
    { day: "Tue", amount: 750 },
    { day: "Wed", amount: 300 },
    { day: "Thu", amount: 900 },
    { day: "Fri", amount: 600 },
    { day: "Sat", amount: 1200 },
    { day: "Sun", amount: 820 },
  ];

  it("renders the Revenue heading", () => {
    render(<RevenueChart points={points} />);
    expect(screen.getByText("Revenue")).toBeInTheDocument();
  });

  it("renders an svg with the correct aria-label", () => {
    const { container } = render(<RevenueChart points={points} />);
    const svg = container.querySelector("svg");
    expect(svg).toHaveAttribute("role", "img");
    expect(svg).toHaveAttribute("aria-label", "Revenue last 7 days");
  });

  it("renders a bar (rect) for each data point", () => {
    const { container } = render(<RevenueChart points={points} />);
    const rects = container.querySelectorAll("rect");
    expect(rects).toHaveLength(points.length);
  });

  it("renders day labels for each point", () => {
    render(<RevenueChart points={points} />);
    for (const p of points) {
      expect(screen.getByText(p.day)).toBeInTheDocument();
    }
  });

  it("renders with a single point without error", () => {
    const { container } = render(
      <RevenueChart points={[{ day: "Mon", amount: 100 }]} />,
    );
    expect(container.querySelector("svg")).toBeInTheDocument();
    expect(container.querySelectorAll("rect")).toHaveLength(1);
  });

  it("renders with all-zero amounts without error", () => {
    const zeroPoints: RevenuePoint[] = [
      { day: "Mon", amount: 0 },
      { day: "Tue", amount: 0 },
    ];
    const { container } = render(<RevenueChart points={zeroPoints} />);
    expect(container.querySelector("svg")).toBeInTheDocument();
  });
});
