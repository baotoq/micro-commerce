import { render } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { Sparkline } from "@/components/seller/shared/sparkline";

describe("Sparkline", () => {
  it("renders an svg with aria-label", () => {
    const { container } = render(<Sparkline points={[10, 20, 30]} />);
    const svg = container.querySelector("svg");
    expect(svg).toBeInTheDocument();
    expect(svg).toHaveAttribute("aria-label", "Sparkline chart");
    expect(svg).toHaveAttribute("role", "img");
  });

  it("renders two paths: area fill and line stroke", () => {
    const { container } = render(<Sparkline points={[5, 10, 15]} />);
    const paths = container.querySelectorAll("path");
    expect(paths).toHaveLength(2);
  });

  it("renders with a single point without error", () => {
    const { container } = render(<Sparkline points={[42]} />);
    expect(container.querySelector("svg")).toBeInTheDocument();
  });

  it("renders with all-zero points without error", () => {
    const { container } = render(<Sparkline points={[0, 0, 0]} />);
    expect(container.querySelector("svg")).toBeInTheDocument();
  });

  it("applies custom className", () => {
    const { container } = render(
      <Sparkline points={[1, 2, 3]} className="h-10 w-20 text-red-500" />,
    );
    expect(container.querySelector("svg")).toHaveClass(
      "h-10",
      "w-20",
      "text-red-500",
    );
  });

  it("uses default className when none provided", () => {
    const { container } = render(<Sparkline points={[1, 2]} />);
    expect(container.querySelector("svg")).toHaveClass(
      "h-8",
      "w-full",
      "text-[#0066cc]",
    );
  });
});
