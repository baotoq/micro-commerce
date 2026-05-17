// web/src/components/seller/listings-table.test.tsx
import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { ListingsTable } from "@/components/seller/listings-table";
import type { Listing } from "@/lib/seller/types";

const make = (n: number): Listing[] =>
  Array.from({ length: n }, (_, i) => ({
    sku: `MC-XX-${String(i + 1).padStart(3, "0")}`,
    name: `Item ${i + 1}`,
    category: "Vessels",
    price: 10,
    inventory: 5,
    status: "active" as const,
    views7d: 0,
  }));

describe("ListingsTable", () => {
  it("renders the rows from the current page and the 'X of Y shown' footer", () => {
    render(<ListingsTable listings={make(9)} total={42} pageSize={9} />);
    expect(screen.getAllByRole("row")).toHaveLength(9 + 1); // 9 body + 1 header
    expect(screen.getByText("9 of 42 shown")).toBeInTheDocument();
  });

  it("falls back to listings.length when total is omitted", () => {
    render(<ListingsTable listings={make(5)} pageSize={9} />);
    expect(screen.getAllByRole("row")).toHaveLength(5 + 1);
    expect(screen.getByText("5 of 5 shown")).toBeInTheDocument();
  });

  it("links pagination controls to ?page=N (page 1 omits the query)", () => {
    render(<ListingsTable listings={make(9)} total={42} pageSize={9} />);
    expect(screen.getByRole("button", { name: "2" })).toHaveAttribute(
      "href",
      "?page=2",
    );
    expect(
      screen.getByRole("button", { name: /go to next page/i }),
    ).toHaveAttribute("href", "?page=2");
  });

  it("disables prev on page 1 and next on the last page", () => {
    const { unmount } = render(
      <ListingsTable
        listings={make(9)}
        total={42}
        pageSize={9}
        currentPage={1}
      />,
    );
    const prev = screen.getByRole("button", { name: /go to previous page/i });
    expect(prev).toHaveAttribute("aria-disabled");
    expect(prev).not.toHaveAttribute("href");
    unmount();
    render(
      <ListingsTable
        listings={make(6)}
        total={42}
        pageSize={9}
        currentPage={5}
      />,
    );
    const next = screen.getByRole("button", { name: /go to next page/i });
    expect(next).toHaveAttribute("aria-disabled");
    expect(next).not.toHaveAttribute("href");
    expect(
      screen.getByRole("button", { name: /go to previous page/i }),
    ).toHaveAttribute("href", "?page=4");
  });

  it("marks the current page with aria-current", () => {
    render(
      <ListingsTable
        listings={make(9)}
        total={42}
        pageSize={9}
        currentPage={2}
      />,
    );
    expect(screen.getByRole("button", { name: "2" })).toHaveAttribute(
      "aria-current",
      "page",
    );
  });
});
