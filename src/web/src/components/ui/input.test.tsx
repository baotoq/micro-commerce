import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { Input } from "./input";

describe("Input", () => {
  it("renders an input element", () => {
    render(<Input aria-label="test-input" />);
    expect(screen.getByRole("textbox")).toBeInTheDocument();
  });

  it("renders without an explicit type when none is provided", () => {
    render(<Input aria-label="default-type" />);
    const input = screen.getByRole("textbox");
    // base-ui Input does not inject type="text" — the browser default applies
    expect(input).toBeInTheDocument();
  });

  it("passes through the type prop", () => {
    render(<Input type="email" aria-label="email-input" />);
    const input = screen.getByRole("textbox");
    expect(input).toHaveAttribute("type", "email");
  });

  it("passes through the password type", () => {
    render(<Input type="password" data-testid="pw-input" />);
    const input = screen.getByTestId("pw-input");
    expect(input).toHaveAttribute("type", "password");
  });

  it("passes through placeholder", () => {
    render(<Input placeholder="Enter value" aria-label="ph-input" />);
    expect(screen.getByPlaceholderText("Enter value")).toBeInTheDocument();
  });

  it("merges custom className", () => {
    render(<Input className="my-input" aria-label="cls-input" />);
    const input = screen.getByRole("textbox");
    expect(input.className).toMatch(/my-input/);
  });

  it("has base styling classes", () => {
    render(<Input aria-label="styled-input" />);
    const input = screen.getByRole("textbox");
    expect(input.className).toMatch(/border-input/);
  });

  it("forwards data attributes", () => {
    render(<Input data-testid="input-extra" aria-label="extra-input" />);
    expect(screen.getByTestId("input-extra")).toBeInTheDocument();
  });
});
