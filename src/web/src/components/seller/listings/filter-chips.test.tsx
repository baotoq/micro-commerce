// web/src/components/seller/filter-chips.test.tsx
import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { FilterChips } from "@/components/seller/listings/filter-chips";

describe("FilterChips", () => {
  const counts = { total: 42, active: 34, low: 3, out: 1, draft: 4 };
  const noop = () => {};

  it("renders five chips with exact label/count format", () => {
    render(<FilterChips counts={counts} active="all" onSelect={noop} />);
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

  it("exposes hrefs for middle-click / accessibility (no status query for 'all')", () => {
    render(<FilterChips counts={counts} active="all" onSelect={noop} />);
    expect(screen.getByRole("link", { name: /All · 42/ })).toHaveAttribute(
      "href",
      "/seller/listings",
    );
    expect(screen.getByRole("link", { name: /Active · 34/ })).toHaveAttribute(
      "href",
      "/seller/listings?status=active",
    );
    expect(screen.getByRole("link", { name: /Drafts · 4/ })).toHaveAttribute(
      "href",
      "/seller/listings?status=draft",
    );
  });

  it("marks the active chip with aria-current=page", () => {
    render(<FilterChips counts={counts} active="active" onSelect={noop} />);
    expect(screen.getByRole("link", { name: /Active · 34/ })).toHaveAttribute(
      "aria-current",
      "page",
    );
    expect(screen.getByRole("link", { name: /All · 42/ })).not.toHaveAttribute(
      "aria-current",
    );
  });

  it("calls onSelect with the chip key on plain left-click and prevents navigation", () => {
    const onSelect = vi.fn();
    render(<FilterChips counts={counts} active="all" onSelect={onSelect} />);
    const lowChip = screen.getByRole("link", { name: /Low · 3/ });
    const event = new MouseEvent("click", {
      bubbles: true,
      cancelable: true,
      button: 0,
    });
    const dispatched = lowChip.dispatchEvent(event);
    expect(onSelect).toHaveBeenCalledWith("low");
    expect(onSelect).toHaveBeenCalledTimes(1);
    // dispatchEvent returns false when preventDefault was called.
    expect(dispatched).toBe(false);
  });

  it("does NOT call onSelect when the user cmd-clicks (opens in new tab)", () => {
    const onSelect = vi.fn();
    render(<FilterChips counts={counts} active="all" onSelect={onSelect} />);
    const activeChip = screen.getByRole("link", { name: /Active · 34/ });
    fireEvent.click(activeChip, { metaKey: true, button: 0 });
    expect(onSelect).not.toHaveBeenCalled();
  });
});
