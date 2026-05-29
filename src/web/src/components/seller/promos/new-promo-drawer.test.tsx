import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

vi.mock("@/lib/seller/promos/actions", () => ({
  createPromoAction: vi.fn(),
}));

import { NewPromoDrawer } from "@/components/seller/promos/new-promo-drawer";
import * as actions from "@/lib/seller/promos/actions";

describe("NewPromoDrawer", () => {
  beforeEach(() => {
    vi.mocked(actions.createPromoAction).mockResolvedValue({
      ok: true,
      sku: "TESTCODE",
    });
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it("renders the drawer heading", () => {
    render(<NewPromoDrawer />);
    expect(
      screen.getByRole("heading", { name: "New promotion" }),
    ).toBeInTheDocument();
  });

  it("renders the close button", () => {
    render(<NewPromoDrawer />);
    expect(screen.getByRole("button", { name: "Close" })).toBeInTheDocument();
  });

  it("renders the Generate button", () => {
    render(<NewPromoDrawer />);
    expect(
      screen.getByRole("button", { name: /generate/i }),
    ).toBeInTheDocument();
  });

  it("Generate button populates the code input", () => {
    render(<NewPromoDrawer />);
    const input = screen.getByRole("textbox", { name: /promo code/i });
    expect((input as HTMLInputElement).value).toBe("");
    fireEvent.click(screen.getByRole("button", { name: /generate/i }));
    expect((input as HTMLInputElement).value).toHaveLength(8);
    expect((input as HTMLInputElement).value).toMatch(/^[A-Z0-9]+$/);
  });

  it("renders all discount kind options", () => {
    render(<NewPromoDrawer />);
    expect(screen.getByRole("button", { name: /% off/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /\$ off/i })).toBeInTheDocument();
    expect(screen.getByText("Free shipping")).toBeInTheDocument();
    expect(screen.getByText("BOGO")).toBeInTheDocument();
  });

  it("Free shipping and BOGO are aria-disabled", () => {
    render(<NewPromoDrawer />);
    const freeShipping = screen
      .getByText("Free shipping")
      .closest("[aria-disabled]");
    const bogo = screen.getByText("BOGO").closest("[aria-disabled]");
    expect(freeShipping).toHaveAttribute("aria-disabled", "true");
    expect(bogo).toHaveAttribute("aria-disabled", "true");
  });

  it("kind toggle defaults to % off, shows percent value input", () => {
    render(<NewPromoDrawer />);
    expect(screen.getByLabelText(/discount value/i)).toBeInTheDocument();
    expect(screen.getByText(/off the entire order/i)).toBeInTheDocument();
  });

  it("kind toggle switches to $ off, label flips to fixed amount", () => {
    render(<NewPromoDrawer />);
    fireEvent.click(screen.getByRole("button", { name: /\$ off/i }));
    expect(screen.getByLabelText(/discount value/i)).toBeInTheDocument();
    expect(screen.getByText(/fixed amount off/i)).toBeInTheDocument();
  });

  it("renders all who-can-use-it options (static visual)", () => {
    render(<NewPromoDrawer />);
    expect(screen.getByText("Anyone with the code")).toBeInTheDocument();
    expect(screen.getByText("Followers only")).toBeInTheDocument();
    expect(screen.getByText("Specific customers")).toBeInTheDocument();
  });

  it("renders all limit card labels", () => {
    render(<NewPromoDrawer />);
    expect(screen.getByText("Min. order")).toBeInTheDocument();
    expect(screen.getByText("Per buyer")).toBeInTheDocument();
    expect(screen.getByText("Total uses")).toBeInTheDocument();
    expect(screen.getByText("Window")).toBeInTheDocument();
  });

  it("does not render the Forecast callout", () => {
    render(<NewPromoDrawer />);
    expect(screen.queryByText("Forecast")).not.toBeInTheDocument();
  });

  it("renders Save draft and Activate now footer buttons", () => {
    render(<NewPromoDrawer />);
    expect(
      screen.getByRole("button", { name: "Save draft" }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Activate now" }),
    ).toBeInTheDocument();
  });

  it("Save draft submits activateImmediately=false", async () => {
    render(<NewPromoDrawer />);

    const codeInput = screen.getByRole("textbox", { name: /promo code/i });
    fireEvent.change(codeInput, { target: { value: "SAVE10" } });

    const valueInput = screen.getByLabelText(/discount value/i);
    fireEvent.change(valueInput, { target: { value: "10" } });

    const descInput = screen.getByLabelText(/description/i);
    fireEvent.change(descInput, { target: { value: "sitewide" } });

    fireEvent.click(screen.getByRole("button", { name: "Save draft" }));

    await waitFor(() => {
      expect(actions.createPromoAction).toHaveBeenCalledTimes(1);
    });
    const fd = vi.mocked(actions.createPromoAction).mock.calls[0][0];
    expect(fd).toBeInstanceOf(FormData);
    expect(fd.get("activateImmediately")).toBe("false");
    expect(fd.get("code")).toBe("SAVE10");
    expect(fd.get("kind")).toBe("percentage");
  });

  it("Activate now submits activateImmediately=true", async () => {
    render(<NewPromoDrawer />);

    const codeInput = screen.getByRole("textbox", { name: /promo code/i });
    fireEvent.change(codeInput, { target: { value: "NOW10" } });

    const valueInput = screen.getByLabelText(/discount value/i);
    fireEvent.change(valueInput, { target: { value: "10" } });

    const descInput = screen.getByLabelText(/description/i);
    fireEvent.change(descInput, { target: { value: "sitewide" } });

    fireEvent.click(screen.getByRole("button", { name: "Activate now" }));

    await waitFor(() => {
      expect(actions.createPromoAction).toHaveBeenCalledTimes(1);
    });
    const fd = vi.mocked(actions.createPromoAction).mock.calls[0][0];
    expect(fd).toBeInstanceOf(FormData);
    expect(fd.get("activateImmediately")).toBe("true");
  });

  it("switching to $ off submits kind=fixed and fixedAmount", async () => {
    render(<NewPromoDrawer />);

    fireEvent.click(screen.getByRole("button", { name: /\$ off/i }));

    const codeInput = screen.getByRole("textbox", { name: /promo code/i });
    fireEvent.change(codeInput, { target: { value: "FIX10" } });

    const valueInput = screen.getByLabelText(/discount value/i);
    fireEvent.change(valueInput, { target: { value: "10" } });

    const descInput = screen.getByLabelText(/description/i);
    fireEvent.change(descInput, { target: { value: "sitewide" } });

    fireEvent.click(screen.getByRole("button", { name: "Save draft" }));

    await waitFor(() => {
      expect(actions.createPromoAction).toHaveBeenCalledTimes(1);
    });
    const fd = vi.mocked(actions.createPromoAction).mock.calls[0][0];
    expect(fd.get("kind")).toBe("fixed");
    expect(fd.get("fixedAmount")).toBe("10");
    expect(fd.get("percentValue")).toBeNull();
  });
});
