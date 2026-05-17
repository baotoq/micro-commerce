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
  it("renders pageSize rows and the 'X of Y shown' footer", () => {
    render(<ListingsTable listings={make(42)} pageSize={9} />);
    expect(screen.getAllByRole("row")).toHaveLength(9 + 1); // 9 body + 1 header
    expect(screen.getByText("9 of 42 shown")).toBeInTheDocument();
  });

  it("clamps pageSize to total when total < pageSize", () => {
    render(<ListingsTable listings={make(5)} pageSize={9} />);
    expect(screen.getAllByRole("row")).toHaveLength(5 + 1);
    expect(screen.getByText("5 of 5 shown")).toBeInTheDocument();
  });
});
