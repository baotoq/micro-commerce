import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { NewPromoDrawer } from "@/components/seller/new-promo-drawer";

describe("NewPromoDrawer", () => {
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

  it("renders the promo code STUDIO15", () => {
    render(<NewPromoDrawer />);
    expect(screen.getByText("STUDIO15")).toBeInTheDocument();
  });

  it("renders the Generate button", () => {
    render(<NewPromoDrawer />);
    expect(
      screen.getByRole("button", { name: /generate/i }),
    ).toBeInTheDocument();
  });

  it("renders all discount options", () => {
    render(<NewPromoDrawer />);
    expect(screen.getByText("% off")).toBeInTheDocument();
    expect(screen.getByText("$ off")).toBeInTheDocument();
    expect(screen.getByText("Free shipping")).toBeInTheDocument();
    expect(screen.getByText("BOGO")).toBeInTheDocument();
  });

  it("renders the discount value of 15%", () => {
    render(<NewPromoDrawer />);
    expect(screen.getByText("15")).toBeInTheDocument();
    expect(screen.getByText("%")).toBeInTheDocument();
    expect(screen.getByText("off the entire order")).toBeInTheDocument();
  });

  it("renders all who-can-use-it options", () => {
    render(<NewPromoDrawer />);
    expect(screen.getByText("Anyone with the code")).toBeInTheDocument();
    expect(screen.getByText("Followers only")).toBeInTheDocument();
    expect(screen.getByText("Specific customers")).toBeInTheDocument();
  });

  it("renders limit fields", () => {
    render(<NewPromoDrawer />);
    expect(screen.getByText("Min. order")).toBeInTheDocument();
    expect(screen.getByText("Per buyer")).toBeInTheDocument();
    expect(screen.getByText("Total uses")).toBeInTheDocument();
    expect(screen.getByText("Window")).toBeInTheDocument();
  });

  it("renders Forecast callout with revenue estimates", () => {
    render(<NewPromoDrawer />);
    expect(screen.getByText("Forecast")).toBeInTheDocument();
    expect(screen.getByText("~24 redemptions")).toBeInTheDocument();
    expect(screen.getByText("$420–$640")).toBeInTheDocument();
  });

  it("renders footer action buttons", () => {
    render(<NewPromoDrawer />);
    expect(
      screen.getByRole("button", { name: "Save draft" }),
    ).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: /Activate/i }),
    ).toBeInTheDocument();
  });

  it("activate button shows scheduled time", () => {
    render(<NewPromoDrawer />);
    expect(
      screen.getByRole("button", { name: /Activate · Tue 12:00 AM/i }),
    ).toBeInTheDocument();
  });
});
