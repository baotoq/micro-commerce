import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { SellerTopbar } from "@/components/seller/seller-topbar";

describe("SellerTopbar", () => {
  it("renders the title", () => {
    render(<SellerTopbar title="Listings" />);
    expect(
      screen.getByRole("heading", { name: "Listings" }),
    ).toBeInTheDocument();
  });

  it("renders optional subtitle when provided", () => {
    render(<SellerTopbar title="Listings" subtitle="Seller Dashboard" />);
    expect(screen.getByText("Seller Dashboard")).toBeInTheDocument();
  });

  it("does not render subtitle element when omitted", () => {
    render(<SellerTopbar title="Listings" />);
    expect(screen.queryByText("Seller Dashboard")).not.toBeInTheDocument();
  });

  it("renders notifications button", () => {
    render(<SellerTopbar title="Listings" />);
    expect(
      screen.getByRole("button", { name: "Notifications" }),
    ).toBeInTheDocument();
  });

  it("renders owner avatar with signed-in aria-label", () => {
    render(<SellerTopbar title="Listings" />);
    expect(
      screen.getByRole("img", { name: "Signed in as Alex" }),
    ).toBeInTheDocument();
  });

  it("renders owner initial in avatar", () => {
    render(<SellerTopbar title="Listings" />);
    const avatar = screen.getByRole("img", { name: "Signed in as Alex" });
    expect(avatar.textContent).toBe("A");
  });

  it("renders actions slot when provided", () => {
    render(
      <SellerTopbar
        title="Listings"
        actions={<button type="button">Export</button>}
      />,
    );
    expect(screen.getByRole("button", { name: "Export" })).toBeInTheDocument();
  });
});
