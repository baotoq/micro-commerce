// web/src/components/seller/filter-chips.test.tsx
import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { FilterChips } from "@/components/seller/filter-chips";

describe("FilterChips", () => {
  const counts = { total: 42, active: 34, low: 3, out: 1, draft: 4 };

  it("renders five chips with exact label/count format", () => {
    render(<FilterChips counts={counts} active="all" />);
    for (const text of ["All · 42", "Active · 34", "Low · 3", "Out · 1", "Drafts · 4"]) {
      expect(screen.getByText(text)).toBeInTheDocument();
    }
  });

  it("marks the active chip with aria-pressed=true and others with aria-pressed=false", () => {
    render(<FilterChips counts={counts} active="active" />);
    expect(screen.getByRole("button", { name: /Active · 34/ })).toHaveAttribute("aria-pressed", "true");
    expect(screen.getByRole("button", { name: /All · 42/ })).toHaveAttribute("aria-pressed", "false");
  });
});
