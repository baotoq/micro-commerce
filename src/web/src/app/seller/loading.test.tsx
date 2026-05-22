// Smoke tests for audit#4 — the loading/error/not-found files are static
// markup, so a single render assertion per file is enough to catch
// regressions (e.g., missing default export, accidental rename of
// expected props).

import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import SellerError from "@/app/seller/error";
import ListingEditNotFound from "@/app/seller/listings/[sku]/edit/not-found";
import SellerLoading from "@/app/seller/loading";

describe("seller route-level fallbacks (audit#4)", () => {
  it("loading.tsx renders a busy skeleton", () => {
    render(<SellerLoading />);
    const section = screen.getByText(/loading/i).closest("section");
    expect(section).toHaveAttribute("aria-busy", "true");
  });

  it("error.tsx renders the error card and a reset button", () => {
    let resetCalls = 0;
    render(
      <SellerError
        error={Object.assign(new Error("boom"), { digest: "abc123" })}
        reset={() => {
          resetCalls += 1;
        }}
      />,
    );
    expect(screen.getByText(/something went wrong/i)).toBeInTheDocument();
    expect(screen.getByText(/abc123/)).toBeInTheDocument();
    screen.getByRole("button", { name: /try again/i }).click();
    expect(resetCalls).toBe(1);
  });

  it("[sku]/edit/not-found.tsx renders the not-found message + back link", () => {
    render(<ListingEditNotFound />);
    expect(screen.getByText(/listing not found/i)).toBeInTheDocument();
    expect(
      screen.getByRole("link", { name: /back to listings/i }),
    ).toHaveAttribute("href", "/seller/listings");
  });
});
