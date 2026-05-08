import { render } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { Icon } from "@/components/icon";

describe("Icon", () => {
  it("renders an svg with the requested size and stroke width", () => {
    const { container } = render(<Icon n="cart" s={20} sw={2} />);
    const svg = container.querySelector("svg");
    expect(svg).toBeInTheDocument();
    expect(svg).toHaveAttribute("width", "20");
    expect(svg).toHaveAttribute("height", "20");
    expect(svg).toHaveAttribute("stroke-width", "2");
  });

  it("uses currentColor for stroke and is hidden from assistive tech", () => {
    const { container } = render(<Icon n="bag" />);
    const svg = container.querySelector("svg");
    expect(svg).toHaveAttribute("stroke", "currentColor");
    expect(svg).toHaveAttribute("aria-hidden", "true");
  });

  it("renders the path data for the requested icon name", () => {
    const { container } = render(<Icon n="search" />);
    const path = container.querySelector("path");
    expect(path).toHaveAttribute(
      "d",
      "M9 3a6 6 0 1 1 0 12A6 6 0 0 1 9 3zm5 10l3.5 3.5",
    );
  });

  it("forwards a className to the svg", () => {
    const { container } = render(<Icon n="heart" className="ml-2" />);
    expect(container.querySelector("svg")).toHaveClass("ml-2");
  });
});
