import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { afterEach, describe, expect, it, vi } from "vitest";

const routerPush = vi.fn();

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push: routerPush }),
}));

vi.mock("@/lib/seller/listings/actions", () => ({
  createListingAction: vi.fn(),
}));

import { NewListingForm } from "@/components/seller/listings/new-listing-form";
import * as actions from "@/lib/seller/listings/actions";

function renderWithSubmit() {
  return render(
    <>
      <NewListingForm />
      <button type="submit" form="new-listing-form">
        Publish
      </button>
    </>,
  );
}

function fillField(label: RegExp | string, value: string) {
  const input = screen.getByLabelText(label) as HTMLInputElement;
  fireEvent.change(input, { target: { value } });
  return input;
}

describe("NewListingForm", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("renders SKU, Name, Category, Price, Inventory, and Status fields", () => {
    renderWithSubmit();

    expect(screen.getByLabelText(/^sku$/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/^name$/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/^category$/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/^price$/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/total in stock/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/^status$/i)).toBeInTheDocument();
  });

  it("does not flag a field on bare focus+blur (touched but never dirtied)", async () => {
    renderWithSubmit();

    const sku = screen.getByLabelText(/^sku$/i);
    expect(screen.queryByText(/sku is required/i)).not.toBeInTheDocument();

    // Focus then blur without typing — the field is touched but not dirty,
    // so the user shouldn't see an error yet.
    fireEvent.focus(sku);
    fireEvent.blur(sku);

    // Give RHF a tick — if validation were going to fire, it would by now.
    await new Promise((r) => setTimeout(r, 50));
    expect(screen.queryByText(/sku is required/i)).not.toBeInTheDocument();
  });

  it("shows the required error after the user dirties and then clears the field", async () => {
    renderWithSubmit();
    const sku = screen.getByLabelText(/^sku$/i);

    fireEvent.change(sku, { target: { value: "X" } });
    // No error while a value is present.
    expect(screen.queryByText(/sku is required/i)).not.toBeInTheDocument();

    fireEvent.change(sku, { target: { value: "" } });

    await waitFor(() => {
      expect(screen.getByText(/sku is required/i)).toBeInTheDocument();
    });
    // Other fields stay untouched/clean.
    expect(screen.queryByText(/name is required/i)).not.toBeInTheDocument();
    expect(screen.queryByText(/category is required/i)).not.toBeInTheDocument();
  });

  it("clears the required error once the field becomes valid again", async () => {
    renderWithSubmit();
    const sku = screen.getByLabelText(/^sku$/i) as HTMLInputElement;

    fireEvent.change(sku, { target: { value: "X" } });
    fireEvent.change(sku, { target: { value: "" } });
    await waitFor(() => {
      expect(screen.getByText(/sku is required/i)).toBeInTheDocument();
    });

    fireEvent.change(sku, { target: { value: "MC-VS-009" } });

    await waitFor(() => {
      expect(screen.queryByText(/sku is required/i)).not.toBeInTheDocument();
    });
  });

  it("shows per-field validation errors when submitting empty form", async () => {
    renderWithSubmit();

    fireEvent.click(screen.getByRole("button", { name: /publish/i }));

    await waitFor(() => {
      expect(screen.getByText(/sku is required/i)).toBeInTheDocument();
    });
    expect(screen.getByText(/name is required/i)).toBeInTheDocument();
    expect(screen.getByText(/category is required/i)).toBeInTheDocument();
    expect(actions.createListingAction).not.toHaveBeenCalled();
  });

  it("flags Price and Inventory as required when submitted empty", async () => {
    renderWithSubmit();

    fillField(/^sku$/i, "MC-VS-009");
    fillField(/^name$/i, "Persimmon vase");
    fillField(/^category$/i, "Ceramics");
    // Leave Price and Inventory blank — z.coerce.number would otherwise turn ""
    // into 0 and silently pass, hiding the required-field state from the user.

    fireEvent.click(screen.getByRole("button", { name: /publish/i }));

    await waitFor(() => {
      expect(screen.getByText(/price is required/i)).toBeInTheDocument();
    });
    expect(screen.getByText(/inventory is required/i)).toBeInTheDocument();
    expect(actions.createListingAction).not.toHaveBeenCalled();
  });

  it("preserves the other fields when one mandatory field is missing (the reported bug)", async () => {
    renderWithSubmit();

    const sku = fillField(/^sku$/i, "MC-VS-009");
    const name = fillField(/^name$/i, "Persimmon vase");
    fillField(/^price$/i, "75");
    fillField(/total in stock/i, "4");
    // Intentionally leave Category blank — this is the reported failure mode.

    fireEvent.click(screen.getByRole("button", { name: /publish/i }));

    await waitFor(() => {
      expect(screen.getByText(/category is required/i)).toBeInTheDocument();
    });
    // The bug pre-fix: SKU and Name went blank after submitting with a missing
    // mandatory field. After the react-hook-form refactor, all values are
    // client-managed and must survive the failed submit.
    expect(sku.value).toBe("MC-VS-009");
    expect(name.value).toBe("Persimmon vase");
    expect(actions.createListingAction).not.toHaveBeenCalled();
  });

  it("calls createListingAction with a FormData payload and navigates on success", async () => {
    vi.mocked(actions.createListingAction).mockResolvedValueOnce({
      ok: true,
      sku: "MC-VS-009",
    });

    renderWithSubmit();

    fillField(/^sku$/i, "mc-vs-009");
    fillField(/^name$/i, "Persimmon vase");
    fillField(/^category$/i, "Ceramics");
    fillField(/^price$/i, "75");
    fillField(/total in stock/i, "4");

    fireEvent.click(screen.getByRole("button", { name: /publish/i }));

    await waitFor(() => {
      expect(actions.createListingAction).toHaveBeenCalledTimes(1);
    });
    const fd = vi.mocked(actions.createListingAction).mock.calls[0][0];
    expect(fd).toBeInstanceOf(FormData);
    // zodResolver runs the schema's .trim().toUpperCase() transforms on the
    // client before onSubmit fires, so the action receives the already-
    // normalised SKU. The server re-parses with the same schema for safety.
    expect(fd.get("sku")).toBe("MC-VS-009");
    expect(fd.get("name")).toBe("Persimmon vase");
    expect(fd.get("category")).toBe("Ceramics");
    expect(fd.get("price")).toBe("75");
    expect(fd.get("inventory")).toBe("4");
    expect(fd.get("status")).toBe("draft");

    await waitFor(() => {
      expect(routerPush).toHaveBeenCalledWith("/seller/listings");
    });
  });

  it("surfaces a server fieldError on the matching field", async () => {
    vi.mocked(actions.createListingAction).mockResolvedValueOnce({
      ok: false,
      error: "Invalid input",
      fieldErrors: { sku: ["A listing with that SKU already exists."] },
    });

    renderWithSubmit();

    fillField(/^sku$/i, "MC-VS-001");
    fillField(/^name$/i, "Persimmon vase");
    fillField(/^category$/i, "Ceramics");
    fillField(/^price$/i, "75");
    fillField(/total in stock/i, "4");

    fireEvent.click(screen.getByRole("button", { name: /publish/i }));

    await waitFor(() => {
      expect(
        screen.getByText(/a listing with that sku already exists/i),
      ).toBeInTheDocument();
    });
    expect(routerPush).not.toHaveBeenCalled();
  });

  it("shows the global error banner when the action returns a non-field error", async () => {
    vi.mocked(actions.createListingAction).mockResolvedValueOnce({
      ok: false,
      error: "A listing with that SKU already exists.",
    });

    renderWithSubmit();

    fillField(/^sku$/i, "MC-VS-001");
    fillField(/^name$/i, "Persimmon vase");
    fillField(/^category$/i, "Ceramics");
    fillField(/^price$/i, "75");
    fillField(/total in stock/i, "4");

    fireEvent.click(screen.getByRole("button", { name: /publish/i }));

    await waitFor(() => {
      expect(screen.getByRole("alert", { name: "" }).textContent).toMatch(
        /already exists/i,
      );
    });
    expect(routerPush).not.toHaveBeenCalled();
  });
});
