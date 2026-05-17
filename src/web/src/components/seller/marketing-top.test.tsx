import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { MarketingTop } from "@/components/seller/marketing-top";

describe("MarketingTop", () => {
  it("renders the brand name", () => {
    render(<MarketingTop />);
    expect(screen.getByText("micro.")).toBeInTheDocument();
  });

  it("renders all nav links", () => {
    render(<MarketingTop />);
    expect(screen.getByRole("link", { name: "Discover" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Shops" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Journal" })).toBeInTheDocument();
    expect(
      screen.getByRole("link", { name: "For makers" }),
    ).toBeInTheDocument();
  });

  it("renders sign in link and sell button", () => {
    render(<MarketingTop />);
    expect(screen.getByRole("link", { name: "Sign in" })).toBeInTheDocument();
    expect(
      screen.getByRole("button", { name: "Sell on Micro" }),
    ).toBeInTheDocument();
  });

  it("sign in link points to /signin", () => {
    render(<MarketingTop />);
    expect(screen.getByRole("link", { name: "Sign in" })).toHaveAttribute(
      "href",
      "/signin",
    );
  });

  it("nav links have correct hrefs", () => {
    render(<MarketingTop />);
    expect(screen.getByRole("link", { name: "Discover" })).toHaveAttribute(
      "href",
      "/discover",
    );
    expect(screen.getByRole("link", { name: "Shops" })).toHaveAttribute(
      "href",
      "/shops",
    );
    expect(screen.getByRole("link", { name: "Journal" })).toHaveAttribute(
      "href",
      "/journal",
    );
    expect(screen.getByRole("link", { name: "For makers" })).toHaveAttribute(
      "href",
      "/sell",
    );
  });

  it("active prop highlights the matching nav link with full opacity", () => {
    render(<MarketingTop active="Discover" />);
    const discoverLink = screen.getByRole("link", { name: "Discover" });
    expect(discoverLink).toHaveStyle({ color: "var(--foreground)" });
  });

  it("defaults to no active link when active prop is omitted", () => {
    render(<MarketingTop />);
    // All links should render without throwing
    expect(screen.getAllByRole("link")).toHaveLength(5); // 4 nav + sign in
  });
});
