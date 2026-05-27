import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

const routerPush = vi.fn();
const routerReplace = vi.fn();
// Drives the mocked searchParams so router.replace?step=2 actually flips the
// hook's return value — otherwise the wizard stays stuck on step 1.
let currentStep: string | null = null;
const searchParamsGet = vi.fn((key: string) =>
  key === "step" ? currentStep : null,
);

vi.mock("next/navigation", () => ({
  useRouter: () => ({
    push: routerPush,
    replace: (url: string) => {
      routerReplace(url);
      const match = /step=(\d)/.exec(url);
      currentStep = match ? match[1] : null;
    },
  }),
  useSearchParams: () => ({
    get: searchParamsGet,
    toString: () => (currentStep ? `step=${currentStep}` : ""),
  }),
  usePathname: () => "/seller/listings/new",
}));

vi.mock("@/lib/seller/listings/actions", () => ({
  createListingAction: vi.fn(),
}));

import { NewListingForm } from "@/components/seller/listings/new-listing-form";
import * as actions from "@/lib/seller/listings/actions";

const fetchMock = vi.fn();

function fillField(label: RegExp | string, value: string) {
  const input = screen.getByLabelText(label) as HTMLInputElement;
  fireEvent.change(input, { target: { value } });
  return input;
}

function fillStep1(skuValue = "MC-VS-009") {
  fillField(/^sku$/i, skuValue);
  fillField(/^name$/i, "Persimmon vase");
  fillField(/^category$/i, "Ceramics");
}

function fillStep2() {
  fillField(/^price$/i, "75");
  fillField(/total in stock/i, "4");
  fillField(/weight \(kg\)/i, "1.25");
  fillField(/^origin$/i, "Portland, OR");
}

describe("NewListingForm (wizard)", () => {
  beforeEach(() => {
    currentStep = null;
    fetchMock.mockReset();
    fetchMock.mockResolvedValue({ status: 404 });
    vi.stubGlobal("fetch", fetchMock);
  });

  afterEach(() => {
    vi.clearAllMocks();
    vi.unstubAllGlobals();
  });

  it("renders the wizard progress strip with 3 steps", () => {
    render(<NewListingForm />);
    expect(screen.getByText(/basics/i)).toBeInTheDocument();
    expect(screen.getByText(/pricing & inventory/i)).toBeInTheDocument();
    expect(screen.getByText(/media & discovery/i)).toBeInTheDocument();
  });

  it("starts on step 1 (Basics) showing SKU/Name/Category", () => {
    render(<NewListingForm />);
    expect(screen.getByLabelText(/^sku$/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/^name$/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/^category$/i)).toBeInTheDocument();
    expect(screen.queryByLabelText(/^price$/i)).not.toBeInTheDocument();
  });

  it("disables Next until step 1 fields are valid", () => {
    render(<NewListingForm />);
    const next = screen.getByRole("button", { name: /next/i });
    expect(next).toBeDisabled();

    fillStep1();
    expect(next).not.toBeDisabled();
  });

  it("Next button is type=button (PRD §8 a11y)", () => {
    render(<NewListingForm />);
    fillStep1();
    const next = screen.getByRole("button", { name: /next/i });
    expect(next).toHaveAttribute("type", "button");
  });

  it("advances to step 2 on Next click and preserves step-1 values", async () => {
    render(<NewListingForm />);
    fillStep1();
    fireEvent.click(screen.getByRole("button", { name: /next/i }));

    await waitFor(() => {
      expect(screen.getByLabelText(/^price$/i)).toBeInTheDocument();
    });
    expect(routerReplace).toHaveBeenCalledWith(expect.stringMatching(/step=2/));

    fireEvent.click(screen.getByRole("button", { name: /back/i }));
    await waitFor(() => {
      expect(screen.getByLabelText(/^sku$/i)).toBeInTheDocument();
    });
    expect((screen.getByLabelText(/^sku$/i) as HTMLInputElement).value).toBe(
      "MC-VS-009",
    );
  });

  it("Back button is type=button and never blocks", async () => {
    render(<NewListingForm />);
    fillStep1();
    fireEvent.click(screen.getByRole("button", { name: /next/i }));
    await screen.findByLabelText(/^price$/i);
    const back = screen.getByRole("button", { name: /back/i });
    expect(back).toHaveAttribute("type", "button");
    expect(back).not.toBeDisabled();
  });

  it("Publish only renders on step 3", async () => {
    render(<NewListingForm />);
    expect(screen.queryByRole("button", { name: /publish/i })).toBeNull();

    fillStep1();
    fireEvent.click(screen.getByRole("button", { name: /next/i }));
    await screen.findByLabelText(/^price$/i);
    expect(screen.queryByRole("button", { name: /publish/i })).toBeNull();

    fillStep2();
    fireEvent.click(screen.getByRole("button", { name: /next/i }));
    await waitFor(() => {
      expect(screen.getByTestId("photo-uploader-stub")).toBeInTheDocument();
    });
    expect(
      screen.getByRole("button", { name: /publish/i }),
    ).toBeInTheDocument();
  });

  it("submits with all fields via FormData on step 3", async () => {
    vi.mocked(actions.createListingAction).mockResolvedValueOnce({
      ok: true,
      sku: "MC-VS-009",
    });

    render(<NewListingForm />);
    fillStep1();
    fireEvent.click(screen.getByRole("button", { name: /next/i }));
    await screen.findByLabelText(/^price$/i);
    fillStep2();
    fireEvent.click(screen.getByRole("button", { name: /next/i }));
    await waitFor(() => {
      expect(screen.getByTestId("photo-uploader-stub")).toBeInTheDocument();
    });

    fireEvent.click(screen.getByRole("button", { name: /publish/i }));

    await waitFor(() => {
      expect(actions.createListingAction).toHaveBeenCalledTimes(1);
    });
    const fd = vi.mocked(actions.createListingAction).mock.calls[0][0];
    expect(fd).toBeInstanceOf(FormData);
    expect(fd.get("sku")).toBe("MC-VS-009");
    expect(fd.get("name")).toBe("Persimmon vase");
    expect(fd.get("category")).toBe("Ceramics");
    expect(fd.get("price")).toBe("75");
    expect(fd.get("inventory")).toBe("4");
    expect(fd.get("weight")).toBe("1.25");
    expect(fd.get("origin")).toBe("Portland, OR");
    expect(fd.get("status")).toBe("draft");

    await waitFor(() => {
      expect(routerPush).toHaveBeenCalledWith("/seller/listings");
    });
  });

  it("renders the dynamic listing-health score (replaces static 92)", () => {
    render(<NewListingForm />);
    expect(screen.getByTestId("listing-health-score")).toHaveTextContent("0");
  });

  it("listing-health updates as the user fills fields", async () => {
    render(<NewListingForm />);
    expect(screen.getByTestId("listing-health-score")).toHaveTextContent("0");
    fillStep1();
    await waitFor(() => {
      const score = Number(
        screen.getByTestId("listing-health-score").textContent,
      );
      // name (15) + category (10) = 25 minimum
      expect(score).toBeGreaterThanOrEqual(25);
    });
  });

  it("debounces SKU uniqueness fetch (single call per 350ms pause)", async () => {
    vi.useFakeTimers();
    try {
      render(<NewListingForm />);
      const sku = screen.getByLabelText(/^sku$/i);
      fireEvent.change(sku, { target: { value: "M" } });
      fireEvent.change(sku, { target: { value: "MC" } });
      fireEvent.change(sku, { target: { value: "MC-" } });
      fireEvent.change(sku, { target: { value: "MC-001" } });

      expect(fetchMock).not.toHaveBeenCalled();
      vi.advanceTimersByTime(360);
      await Promise.resolve();
      expect(fetchMock).toHaveBeenCalledTimes(1);
      expect(fetchMock.mock.calls[0][0]).toMatch(
        /\/api\/products\/by-sku\/MC-001\/exists$/,
      );
    } finally {
      vi.useRealTimers();
    }
  });

  it("does not call SKU fetch when SKU is empty", async () => {
    vi.useFakeTimers();
    try {
      render(<NewListingForm />);
      vi.advanceTimersByTime(500);
      await Promise.resolve();
      expect(fetchMock).not.toHaveBeenCalled();
    } finally {
      vi.useRealTimers();
    }
  });

  it("surfaces a server fieldError after submit", async () => {
    vi.mocked(actions.createListingAction).mockResolvedValueOnce({
      ok: false,
      error: "Invalid input",
      fieldErrors: { sku: ["A listing with that SKU already exists."] },
    });

    render(<NewListingForm />);
    fillStep1();
    fireEvent.click(screen.getByRole("button", { name: /next/i }));
    await screen.findByLabelText(/^price$/i);
    fillStep2();
    fireEvent.click(screen.getByRole("button", { name: /next/i }));
    await waitFor(() => {
      expect(screen.getByTestId("photo-uploader-stub")).toBeInTheDocument();
    });

    fireEvent.click(screen.getByRole("button", { name: /publish/i }));

    await waitFor(() => {
      expect(
        screen.getByText(/a listing with that sku already exists/i),
      ).toBeInTheDocument();
    });
  });
});
