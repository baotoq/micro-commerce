// web/src/components/seller/filter-chips.test.tsx
import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { FilterChips } from "@/components/seller/filter-chips";

describe("FilterChips", () => {
  const counts = { total: 42, active: 34, low: 3, out: 1, draft: 4 };

  it("renders five chips with exact label/count format", () => {
    render(<FilterChips counts={counts} active="all" />);
    for (const text of [
      "All · 42",
      "Active · 34",
      "Low · 3",
      "Out · 1",
      "Drafts · 4",
    ]) {
      expect(screen.getByText(text)).toBeInTheDocument();
    }
  });

  it("renders each chip as a link to the listings page with the matching status query (no status for 'all')", () => {
    render(<FilterChips counts={counts} active="all" />);
    expect(screen.getByRole("link", { name: /All · 42/ })).toHaveAttribute(
      "href",
      "/seller/listings",
    );
    expect(screen.getByRole("link", { name: /Active · 34/ })).toHaveAttribute(
      "href",
      "/seller/listings?status=active",
    );
    expect(screen.getByRole("link", { name: /Low · 3/ })).toHaveAttribute(
      "href",
      "/seller/listings?status=low",
    );
    expect(screen.getByRole("link", { name: /Out · 1/ })).toHaveAttribute(
      "href",
      "/seller/listings?status=out",
    );
    expect(screen.getByRole("link", { name: /Drafts · 4/ })).toHaveAttribute(
      "href",
      "/seller/listings?status=draft",
    );
  });

  it("marks the active chip with aria-current=page and leaves others without it", () => {
    render(<FilterChips counts={counts} active="active" />);
    expect(screen.getByRole("link", { name: /Active · 34/ })).toHaveAttribute(
      "aria-current",
      "page",
    );
    expect(screen.getByRole("link", { name: /All · 42/ })).not.toHaveAttribute(
      "aria-current",
    );
  });
});
