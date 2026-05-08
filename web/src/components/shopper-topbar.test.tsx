import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { ShopperTopbar } from "@/components/shopper-topbar";

describe("ShopperTopbar", () => {
  it("renders the shop name as a link to home", () => {
    render(<ShopperTopbar />);
    const logo = screen.getByRole("link", { name: "Mira Studio" });
    expect(logo).toHaveAttribute("href", "/");
  });

  it("supports a custom shop name", () => {
    render(<ShopperTopbar shop="Other Studio" />);
    expect(
      screen.getByRole("link", { name: "Other Studio" }),
    ).toBeInTheDocument();
  });

  it("renders the four primary nav links with the right hrefs", () => {
    render(<ShopperTopbar />);
    expect(screen.getByRole("link", { name: "Shop" })).toHaveAttribute(
      "href",
      "/",
    );
    for (const label of ["Collections", "Journal", "About"]) {
      expect(screen.getByRole("link", { name: label })).toHaveAttribute(
        "href",
        "#",
      );
    }
  });

  it("links the cart icon to /cart", () => {
    const { container } = render(<ShopperTopbar />);
    const cartLink = container.querySelector('a[href="/cart"]');
    expect(cartLink).toBeInTheDocument();
  });

  it("hides the cart count when zero", () => {
    render(<ShopperTopbar cartCount={0} />);
    expect(screen.queryByText("0")).not.toBeInTheDocument();
  });

  it("shows the cart count badge when positive", () => {
    render(<ShopperTopbar cartCount={3} />);
    expect(screen.getByText("3")).toBeInTheDocument();
  });
});
