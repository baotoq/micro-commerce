import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { CopyShopLink } from "@/components/seller/copy-shop-link";

describe("CopyShopLink", () => {
  it("renders button with domain text", () => {
    render(<CopyShopLink domain="myshop.micro.com" />);
    expect(
      screen.getByRole("button", { name: /Copy myshop\.micro\.com/ }),
    ).toBeInTheDocument();
  });

  it("calls clipboard.writeText with full https URL on click", () => {
    const writeText = vi.fn().mockResolvedValue(undefined);
    Object.assign(navigator, { clipboard: { writeText } });

    render(<CopyShopLink domain="myshop.micro.com" />);
    fireEvent.click(screen.getByRole("button"));

    expect(writeText).toHaveBeenCalledWith("https://myshop.micro.com");
  });

  it("reflects different domain prop", () => {
    render(<CopyShopLink domain="another.shop" />);
    expect(
      screen.getByRole("button", { name: /Copy another\.shop/ }),
    ).toBeInTheDocument();
  });
});
