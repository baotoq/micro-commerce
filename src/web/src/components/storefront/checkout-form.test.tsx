import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";

const placeOrder = vi.fn();
vi.mock("@/lib/storefront/actions", () => ({
  placeOrder: (...a: unknown[]) => placeOrder(...a),
}));

import { CheckoutForm } from "@/components/storefront/checkout-form";

const totals = {
  subtotal: 86,
  discount: 0,
  shipping: 8,
  tax: 7.31,
  total: 101.31,
};

function fillShipping() {
  fireEvent.change(screen.getByLabelText("Full name"), {
    target: { value: "Bao Buyer" },
  });
  fireEvent.change(screen.getByLabelText("Street address"), {
    target: { value: "241 Telegraph Ave" },
  });
  fireEvent.change(screen.getByLabelText("City, State ZIP"), {
    target: { value: "Oakland, CA 94612" },
  });
}

describe("CheckoutForm", () => {
  beforeEach(() => vi.clearAllMocks());

  it("walks Ship → Pay → Review and submits the typed payload", async () => {
    placeOrder.mockResolvedValue({ ok: false, error: "UNKNOWN" }); // redirect happens server-side
    render(<CheckoutForm email="buyer@microcommerce.dev" totals={totals} />);

    fillShipping();
    fireEvent.click(
      screen.getByRole("button", { name: /Continue to payment/ }),
    );

    fireEvent.change(await screen.findByLabelText("Card number"), {
      target: { value: "4242 4242 4242 4242" },
    });
    fireEvent.change(screen.getByLabelText("Name on card"), {
      target: { value: "Bao Buyer" },
    });
    fireEvent.click(screen.getByRole("button", { name: /Review order/ }));

    fireEvent.click(await screen.findByRole("button", { name: /Place order/ }));

    await waitFor(() =>
      expect(placeOrder).toHaveBeenCalledWith(
        expect.objectContaining({
          customerName: "Bao Buyer",
          shipLine1: "241 Telegraph Ave",
          cityState: "Oakland, CA 94612",
          shippingId: "standard",
          paymentBrand: "Visa",
          paymentLastFour: "4242",
        }),
      ),
    );
  });

  it("blocks the ship step until required fields are filled", async () => {
    render(<CheckoutForm email="b@x.dev" totals={totals} />);
    fireEvent.click(
      screen.getByRole("button", { name: /Continue to payment/ }),
    );
    expect(await screen.findAllByRole("alert")).not.toHaveLength(0);
    expect(screen.queryByLabelText("Card number")).not.toBeInTheDocument();
  });

  it("surfaces a server rejection", async () => {
    placeOrder.mockResolvedValue({ ok: false, error: "INSUFFICIENT_STOCK" });
    render(<CheckoutForm email="b@x.dev" totals={totals} />);
    fillShipping();
    fireEvent.click(
      screen.getByRole("button", { name: /Continue to payment/ }),
    );
    fireEvent.change(await screen.findByLabelText("Card number"), {
      target: { value: "4242424242424242" },
    });
    fireEvent.change(screen.getByLabelText("Name on card"), {
      target: { value: "B" },
    });
    fireEvent.click(screen.getByRole("button", { name: /Review order/ }));
    fireEvent.click(await screen.findByRole("button", { name: /Place order/ }));
    expect(await screen.findByText(/no longer in stock/i)).toBeInTheDocument();
  });
});
