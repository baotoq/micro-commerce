import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { Button } from "./button";

describe("Button", () => {
  it("renders with default variant classes", () => {
    render(<Button>Click me</Button>);
    const btn = screen.getByRole("button", { name: "Click me" });
    expect(btn.className).toMatch(/bg-primary/);
  });

  it("applies outline variant classes", () => {
    render(<Button variant="outline">Outline</Button>);
    const btn = screen.getByRole("button", { name: "Outline" });
    expect(btn.className).toMatch(/border-border/);
  });

  it("applies secondary variant classes", () => {
    render(<Button variant="secondary">Secondary</Button>);
    const btn = screen.getByRole("button", { name: "Secondary" });
    expect(btn.className).toMatch(/bg-secondary/);
  });

  it("applies destructive variant classes", () => {
    render(<Button variant="destructive">Delete</Button>);
    const btn = screen.getByRole("button", { name: "Delete" });
    expect(btn.className).toMatch(/bg-destructive/);
  });

  it("applies ghost variant classes", () => {
    render(<Button variant="ghost">Ghost</Button>);
    const btn = screen.getByRole("button", { name: "Ghost" });
    expect(btn.className).toMatch(/hover:bg-muted/);
  });

  it("applies link variant classes", () => {
    render(<Button variant="link">Link</Button>);
    const btn = screen.getByRole("button", { name: "Link" });
    expect(btn.className).toMatch(/text-primary/);
  });

  it("applies sm size classes", () => {
    render(<Button size="sm">Small</Button>);
    const btn = screen.getByRole("button", { name: "Small" });
    expect(btn.className).toMatch(/h-7/);
  });

  it("applies lg size classes", () => {
    render(<Button size="lg">Large</Button>);
    const btn = screen.getByRole("button", { name: "Large" });
    expect(btn.className).toMatch(/h-9/);
  });

  it("applies icon size classes", () => {
    render(<Button size="icon" aria-label="icon-btn" />);
    const btn = screen.getByRole("button", { name: "icon-btn" });
    expect(btn.className).toMatch(/size-8/);
  });

  it("merges custom className", () => {
    render(<Button className="my-custom-class">Custom</Button>);
    const btn = screen.getByRole("button", { name: "Custom" });
    expect(btn.className).toMatch(/my-custom-class/);
  });

  it("has button role", () => {
    render(<Button>Role check</Button>);
    expect(
      screen.getByRole("button", { name: "Role check" }),
    ).toBeInTheDocument();
  });

  it("forwards extra props", () => {
    render(<Button data-testid="btn-extra">Extra</Button>);
    expect(screen.getByTestId("btn-extra")).toBeInTheDocument();
  });
});
