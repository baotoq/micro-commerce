import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { Badge } from "./badge";

describe("Badge", () => {
  it("renders children", () => {
    render(<Badge>New</Badge>);
    expect(screen.getByText("New")).toBeInTheDocument();
  });

  it("renders as a span by default", () => {
    render(<Badge>Tag</Badge>);
    const el = screen.getByText("Tag");
    expect(el.tagName.toLowerCase()).toBe("span");
  });

  it("applies default variant classes", () => {
    render(<Badge>Default</Badge>);
    const el = screen.getByText("Default");
    expect(el.className).toMatch(/bg-primary/);
  });

  it("applies secondary variant classes", () => {
    render(<Badge variant="secondary">Secondary</Badge>);
    const el = screen.getByText("Secondary");
    expect(el.className).toMatch(/bg-secondary/);
  });

  it("applies destructive variant classes", () => {
    render(<Badge variant="destructive">Error</Badge>);
    const el = screen.getByText("Error");
    expect(el.className).toMatch(/bg-destructive/);
  });

  it("applies outline variant classes", () => {
    render(<Badge variant="outline">Outline</Badge>);
    const el = screen.getByText("Outline");
    expect(el.className).toMatch(/border-border/);
  });

  it("applies ghost variant classes", () => {
    render(<Badge variant="ghost">Ghost</Badge>);
    const el = screen.getByText("Ghost");
    expect(el.className).toMatch(/hover:bg-muted/);
  });

  it("merges custom className", () => {
    render(<Badge className="custom-badge">Custom</Badge>);
    const el = screen.getByText("Custom");
    expect(el.className).toMatch(/custom-badge/);
  });

  it("forwards extra props", () => {
    render(<Badge data-testid="badge-extra">Extra</Badge>);
    expect(screen.getByTestId("badge-extra")).toBeInTheDocument();
  });
});
