import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { SellerSidebar } from "@/components/seller/shell/sidebar";

vi.mock("next/navigation", () => ({ usePathname: () => "/seller" }));

describe("SellerSidebar", () => {
  it("renders brand name", () => {
    render(<SellerSidebar />);
    expect(screen.getByText("Micro Commerce")).toBeInTheDocument();
  });

  it("renders brand initial avatar", () => {
    render(<SellerSidebar />);
    expect(screen.getByText("M")).toBeInTheDocument();
  });

  it("renders plan label", () => {
    render(<SellerSidebar />);
    expect(screen.getByText("Plan · Maker")).toBeInTheDocument();
  });

  it("renders all nav labels", () => {
    render(<SellerSidebar />);
    for (const label of [
      "Overview",
      "Orders",
      "Listings",
      "Discounts",
      "Analytics",
      "Customers",
    ]) {
      expect(screen.getByText(label)).toBeInTheDocument();
    }
  });

  it("renders setup progress card", () => {
    render(<SellerSidebar />);
    expect(screen.getByText("Setup · 4 of 6")).toBeInTheDocument();
    expect(screen.getByText("Add payouts & ship rates")).toBeInTheDocument();
  });
});
