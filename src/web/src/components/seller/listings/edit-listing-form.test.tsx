import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";

vi.mock("next/navigation", () => ({
  useRouter: () => ({ refresh: vi.fn() }),
}));

vi.mock("@/lib/seller/listings/actions", () => ({
  updateListingAction: vi.fn(),
}));

import { EditListingForm } from "@/components/seller/listings/edit-listing-form";
import * as actions from "@/lib/seller/listings/actions";
import type { Listing } from "@/lib/seller/listings/types";

const listing: Listing = {
  sku: "TEST-001",
  name: "Test Vase",
  category: "Vessels",
  price: 45.0,
  inventory: 8,
  status: "active",
  views7d: 12,
};

describe("EditListingForm", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("renders fields pre-filled with listing values", () => {
    render(<EditListingForm listing={listing} />);

    expect(screen.getByDisplayValue("Test Vase")).toBeInTheDocument();
    expect(screen.getByDisplayValue("Vessels")).toBeInTheDocument();
    expect(screen.getByDisplayValue("45")).toBeInTheDocument();
    expect(screen.getByDisplayValue("8")).toBeInTheDocument();
    // SKU shown read-only
    expect(screen.getByDisplayValue("TEST-001")).toBeInTheDocument();
  });

  it("SKU input is read-only", () => {
    render(<EditListingForm listing={listing} />);
    const skuInput = screen.getByDisplayValue("TEST-001");
    expect(skuInput).toHaveAttribute("readonly");
  });

  it("shows a save button", () => {
    render(<EditListingForm listing={listing} />);
    expect(screen.getByRole("button", { name: /save/i })).toBeInTheDocument();
  });

  it("shows success banner when action returns ok:true", async () => {
    vi.mocked(actions.updateListingAction).mockResolvedValueOnce({
      ok: true,
      sku: "TEST-001",
    });

    render(<EditListingForm listing={listing} />);

    fireEvent.click(screen.getByRole("button", { name: /save/i }));

    await waitFor(() => {
      expect(screen.getByText(/saved/i)).toBeInTheDocument();
    });
  });

  it("shows top-level error when action returns ok:false", async () => {
    vi.mocked(actions.updateListingAction).mockResolvedValueOnce({
      ok: false,
      error: "Listing not found.",
    });

    render(<EditListingForm listing={listing} />);

    fireEvent.click(screen.getByRole("button", { name: /save/i }));

    await waitFor(() => {
      expect(screen.getByText("Listing not found.")).toBeInTheDocument();
    });
  });

  it("shows field errors when action returns fieldErrors", async () => {
    vi.mocked(actions.updateListingAction).mockResolvedValueOnce({
      ok: false,
      error: "Invalid input",
      fieldErrors: { price: ["Price must be non-negative"] },
    });

    render(<EditListingForm listing={listing} />);

    fireEvent.click(screen.getByRole("button", { name: /save/i }));

    await waitFor(() => {
      expect(
        screen.getByText("Price must be non-negative"),
      ).toBeInTheDocument();
    });
  });
});
