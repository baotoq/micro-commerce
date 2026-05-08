import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { Avatar, ProductImage, Stars } from "@/components/primitives";

describe("Stars", () => {
  it("renders `of` star slots", () => {
    const { container } = render(<Stars n={3} of={5} />);
    expect(container.querySelectorAll(".hf-star")).toHaveLength(5);
  });

  it("colors the first n slots with --sun and the rest with --ink-5", () => {
    const { container } = render(<Stars n={2} of={4} />);
    const stars = Array.from(container.querySelectorAll(".hf-star"));
    expect(stars[0]).toHaveStyle({ color: "var(--sun)" });
    expect(stars[1]).toHaveStyle({ color: "var(--sun)" });
    expect(stars[2]).toHaveStyle({ color: "var(--ink-5)" });
    expect(stars[3]).toHaveStyle({ color: "var(--ink-5)" });
  });

  it("applies size to the wrapper font-size and each star", () => {
    const { container } = render(<Stars n={1} of={1} size={20} />);
    const wrapper = container.querySelector(".hf-rating");
    expect(wrapper).toHaveStyle({ fontSize: "20px" });
    expect(container.querySelector(".hf-star")).toHaveStyle({
      width: "20px",
      height: "20px",
    });
  });
});

describe("Avatar", () => {
  it("renders up to two uppercase initials from the name", () => {
    const { rerender } = render(<Avatar name="Mira Castellanos" />);
    expect(screen.getByText("MC")).toBeInTheDocument();

    rerender(<Avatar name="sasha" />);
    expect(screen.getByText("S")).toBeInTheDocument();
  });

  it("picks a deterministic background color from the first char", () => {
    const { container, rerender } = render(<Avatar name="Mira" />);
    const first = container.firstElementChild as HTMLElement;
    const firstBg = first.style.background;

    rerender(<Avatar name="Mira" />);
    const second = container.firstElementChild as HTMLElement;
    expect(second.style.background).toBe(firstBg);
  });

  it("applies the size class", () => {
    const { container, rerender } = render(<Avatar name="A" size="sm" />);
    expect(container.firstElementChild).toHaveClass(
      "hf-avatar",
      "hf-avatar-sm",
    );

    rerender(<Avatar name="A" size="lg" />);
    expect(container.firstElementChild).toHaveClass(
      "hf-avatar",
      "hf-avatar-lg",
    );

    rerender(<Avatar name="A" size="md" />);
    expect(container.firstElementChild).toHaveClass("hf-avatar");
    expect(container.firstElementChild).not.toHaveClass(
      "hf-avatar-sm",
      "hf-avatar-lg",
      "hf-avatar-xl",
    );
  });
});

describe("ProductImage", () => {
  it("composes the gradient class from the tone prop", () => {
    const { container } = render(<ProductImage tone="clay" h={200} />);
    expect(container.firstElementChild).toHaveClass("hf-img", "hf-img-clay");
  });

  it("applies the height and default border radius token", () => {
    const { container } = render(<ProductImage tone="sage" h={180} />);
    const el = container.firstElementChild as HTMLElement;
    expect(el).toHaveStyle({
      height: "180px",
      borderRadius: "var(--r-md)",
      width: "100%",
    });
  });

  it("renders the badge when provided and omits it otherwise", () => {
    const { rerender } = render(
      <ProductImage tone="rose" h={120} badge="New" />,
    );
    expect(screen.getByText("New")).toBeInTheDocument();

    rerender(<ProductImage tone="rose" h={120} />);
    expect(screen.queryByText("New")).not.toBeInTheDocument();
  });

  it("renders the label when provided", () => {
    render(<ProductImage tone="moss" h={120} label="01 / 05" />);
    expect(screen.getByText("01 / 05")).toBeInTheDocument();
  });

  it("honors a custom border radius", () => {
    const { container } = render(
      <ProductImage tone="bone" h={100} r="var(--r-xl)" />,
    );
    expect(container.firstElementChild).toHaveStyle({
      borderRadius: "var(--r-xl)",
    });
  });
});
