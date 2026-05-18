import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { SourcesDonut } from "@/components/seller/sources-donut";
import type { SourceBreakdown } from "@/lib/seller/analytics/types";

describe("SourcesDonut", () => {
  const sources: SourceBreakdown[] = [
    { name: "Organic search", visits: 4200, share: 0.5 },
    { name: "Direct", visits: 2100, share: 0.25 },
    { name: "Social", visits: 1260, share: 0.15 },
    { name: "Referral", visits: 840, share: 0.1 },
  ];

  it("renders the Sources heading", () => {
    render(<SourcesDonut sources={sources} />);
    expect(screen.getByText("Sources")).toBeInTheDocument();
  });

  it("renders an svg with the correct aria-label", () => {
    const { container } = render(<SourcesDonut sources={sources} />);
    const svg = container.querySelector("svg");
    expect(svg).toHaveAttribute("role", "img");
    expect(svg).toHaveAttribute("aria-label", "Traffic sources breakdown");
  });

  it("renders a legend entry for each source", () => {
    render(<SourcesDonut sources={sources} />);
    expect(screen.getByText("Organic search")).toBeInTheDocument();
    expect(screen.getByText("Direct")).toBeInTheDocument();
    expect(screen.getByText("Social")).toBeInTheDocument();
    expect(screen.getByText("Referral")).toBeInTheDocument();
  });

  it("renders visit counts formatted with commas", () => {
    render(<SourcesDonut sources={sources} />);
    expect(screen.getByText("4,200")).toBeInTheDocument();
    expect(screen.getByText("2,100")).toBeInTheDocument();
  });

  it("renders total visits in footer", () => {
    render(<SourcesDonut sources={sources} />);
    expect(screen.getByText("8,400 visits")).toBeInTheDocument();
  });

  it("renders a circle for each source in the donut", () => {
    const { container } = render(<SourcesDonut sources={sources} />);
    const circles = container.querySelectorAll("circle");
    expect(circles).toHaveLength(sources.length);
  });

  it("renders correctly with a single source", () => {
    const single: SourceBreakdown[] = [
      { name: "Direct", visits: 100, share: 1.0 },
    ];
    render(<SourcesDonut sources={single} />);
    expect(screen.getByText("Direct")).toBeInTheDocument();
    expect(screen.getByText("100 visits")).toBeInTheDocument();
  });
});
