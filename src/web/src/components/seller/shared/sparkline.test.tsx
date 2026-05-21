import { render } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { Sparkline } from "@/components/seller/shared/sparkline";

describe("Sparkline", () => {
  it("renders an svg with the data-meaningful aria-label from the required `label` prop", () => {
    const { container } = render(
      <Sparkline points={[10, 20, 30]} label="Revenue, last 30 days" />,
    );
    const svg = container.querySelector("svg");
    expect(svg).toBeInTheDocument();
    expect(svg).toHaveAttribute("aria-label", "Revenue, last 30 days");
    expect(svg).toHaveAttribute("role", "img");
  });

  it("renders a <title> element so screen readers and tooltips have a textual summary", () => {
    const { container } = render(
      <Sparkline points={[10, 20, 30]} label="Sales last 7d" />,
    );
    const title = container.querySelector("svg title");
    expect(title).not.toBeNull();
    expect(title?.textContent).toMatch(/Sales last 7d/);
  });

  it("renders two paths: area fill and line stroke", () => {
    const { container } = render(
      <Sparkline points={[5, 10, 15]} label="series" />,
    );
    const paths = container.querySelectorAll("path");
    expect(paths).toHaveLength(2);
  });

  it("renders with a single point without error", () => {
    const { container } = render(<Sparkline points={[42]} label="single" />);
    expect(container.querySelector("svg")).toBeInTheDocument();
  });

  it("renders with all-zero points without error", () => {
    const { container } = render(<Sparkline points={[0, 0, 0]} label="flat" />);
    expect(container.querySelector("svg")).toBeInTheDocument();
  });

  it("applies custom className", () => {
    const { container } = render(
      <Sparkline
        points={[1, 2, 3]}
        label="custom"
        className="h-10 w-20 text-bad"
      />,
    );
    expect(container.querySelector("svg")).toHaveClass(
      "h-10",
      "w-20",
      "text-bad",
    );
  });

  it("uses default text-primary color class when no className override is provided", () => {
    const { container } = render(<Sparkline points={[1, 2]} label="default" />);
    expect(container.querySelector("svg")).toHaveClass(
      "h-8",
      "w-full",
      "text-primary",
    );
  });
});
