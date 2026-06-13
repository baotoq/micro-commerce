import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

const addToCart = vi.fn();
vi.mock("@/lib/storefront/actions", () => ({
  addToCart: (...a: unknown[]) => addToCart(...a),
}));

import { AddToBag } from "@/components/storefront/add-to-bag";

describe("AddToBag", () => {
  beforeEach(() => vi.clearAllMocks());

  it("steps quantity within bounds and submits", async () => {
    addToCart.mockResolvedValue({ ok: true, qty: 2 });
    render(<AddToBag sku="MC-VS-001" price={86} maxQty={3} />);

    fireEvent.click(screen.getByRole("button", { name: "Increase quantity" }));
    expect(screen.getByText("2")).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: /Add to bag/ }));
    await waitFor(() => expect(addToCart).toHaveBeenCalledWith("MC-VS-001", 2));
    expect(await screen.findByText("Added to bag ✓")).toBeInTheDocument();
  });

  it("shows the error state when the action rejects the add", async () => {
    addToCart.mockResolvedValue({ ok: false, error: "UNAVAILABLE" });
    render(<AddToBag sku="MC-VS-001" price={86} maxQty={1} />);
    fireEvent.click(screen.getByRole("button", { name: /Add to bag/ }));
    expect(
      await screen.findByText("This piece just sold out."),
    ).toBeInTheDocument();
  });

  it("never steps above maxQty or below 1", () => {
    render(<AddToBag sku="MC-VS-001" price={86} maxQty={1} />);
    fireEvent.click(screen.getByRole("button", { name: "Increase quantity" }));
    fireEvent.click(screen.getByRole("button", { name: "Decrease quantity" }));
    fireEvent.click(screen.getByRole("button", { name: "Decrease quantity" }));
    expect(screen.getByText("1")).toBeInTheDocument();
  });
});
