// web/src/components/seller/listings-table.test.tsx
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import type { ReactNode } from "react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { ListingsTable } from "@/components/seller/listings-table";
import type { Listing } from "@/lib/seller/listings/types";

const make = (n: number, offset = 0): Listing[] =>
  Array.from({ length: n }, (_, i) => ({
    sku: `MC-XX-${String(i + 1 + offset).padStart(3, "0")}`,
    name: `Item ${i + 1 + offset}`,
    category: "Vessels",
    price: 10,
    inventory: 5,
    status: "active" as const,
    views7d: 0,
  }));

function renderWithQuery(ui: ReactNode) {
  const client = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return render(
    <QueryClientProvider client={client}>{ui}</QueryClientProvider>,
  );
}

describe("ListingsTable", () => {
  it("renders the rows from the current page and the 'X of Y shown' footer", () => {
    renderWithQuery(
      <ListingsTable listings={make(9)} total={42} pageSize={9} />,
    );
    expect(screen.getAllByRole("row")).toHaveLength(9 + 1); // 9 body + 1 header
    expect(screen.getByText("9 of 42 shown")).toBeInTheDocument();
  });

  it("falls back to listings.length when total is omitted", () => {
    renderWithQuery(<ListingsTable listings={make(5)} pageSize={9} />);
    expect(screen.getAllByRole("row")).toHaveLength(5 + 1);
    expect(screen.getByText("5 of 5 shown")).toBeInTheDocument();
  });

  it("links pagination controls to ?page=N (page 1 omits the query)", () => {
    renderWithQuery(
      <ListingsTable listings={make(9)} total={42} pageSize={9} />,
    );
    expect(screen.getByRole("button", { name: "2" })).toHaveAttribute(
      "href",
      "?page=2",
    );
    expect(
      screen.getByRole("button", { name: /go to next page/i }),
    ).toHaveAttribute("href", "?page=2");
  });

  it("disables prev on page 1 and next on the last page", () => {
    const { unmount } = renderWithQuery(
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
    renderWithQuery(
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
    renderWithQuery(
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

describe("ListingsTable client pagination", () => {
  const fetchSpy = vi.fn();
  const replaceStateSpy = vi.spyOn(window.history, "replaceState");

  beforeEach(() => {
    fetchSpy.mockReset();
    replaceStateSpy.mockClear();
    vi.stubGlobal("fetch", fetchSpy);
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  function mockPageResponse(items: Listing[], page: number, total = 42) {
    fetchSpy.mockResolvedValueOnce(
      new Response(JSON.stringify({ items, total, page, pageSize: 9 }), {
        status: 200,
        headers: { "content-type": "application/json" },
      }),
    );
  }

  it("does not fetch on initial render (initialData satisfies the query)", () => {
    renderWithQuery(
      <ListingsTable
        listings={make(9)}
        total={42}
        pageSize={9}
        currentPage={1}
      />,
    );
    expect(fetchSpy).not.toHaveBeenCalled();
  });

  it("fetches the next page via /api/listings and swaps rows in", async () => {
    const page2 = make(9, 9);
    mockPageResponse(page2, 2);

    renderWithQuery(
      <ListingsTable
        listings={make(9)}
        total={42}
        pageSize={9}
        currentPage={1}
      />,
    );

    fireEvent.click(screen.getByRole("button", { name: "2" }));

    await waitFor(() => {
      expect(fetchSpy).toHaveBeenCalledWith("/api/listings?page=2&limit=9");
    });
    expect(
      await screen.findByRole("cell", { name: page2[0].sku }),
    ).toBeInTheDocument();
    expect(screen.queryByRole("cell", { name: "MC-XX-001" })).toBeNull();
  });

  it("updates the URL via history.replaceState — no full reload", async () => {
    mockPageResponse(make(9, 9), 2);

    renderWithQuery(
      <ListingsTable
        listings={make(9)}
        total={42}
        pageSize={9}
        currentPage={1}
      />,
    );

    fireEvent.click(screen.getByRole("button", { name: "2" }));

    await waitFor(() => {
      const lastCall = replaceStateSpy.mock.calls.at(-1);
      expect(lastCall?.[2]).toMatch(/\?page=2$/);
    });
  });

  it("clicking page 1 from page 2 strips the ?page query", async () => {
    mockPageResponse(make(9), 1);

    renderWithQuery(
      <ListingsTable
        listings={make(9, 9)}
        total={42}
        pageSize={9}
        currentPage={2}
      />,
    );

    fireEvent.click(screen.getByRole("button", { name: "1" }));

    await waitFor(() => {
      expect(fetchSpy).toHaveBeenCalledWith("/api/listings?page=1&limit=9");
      const lastCall = replaceStateSpy.mock.calls.at(-1);
      expect(lastCall?.[2]).not.toMatch(/\?page=/);
    });
  });

  it("does not fetch when prev is disabled on page 1", () => {
    renderWithQuery(
      <ListingsTable
        listings={make(9)}
        total={42}
        pageSize={9}
        currentPage={1}
      />,
    );
    fireEvent.click(
      screen.getByRole("button", { name: /go to previous page/i }),
    );
    expect(fetchSpy).not.toHaveBeenCalled();
  });

  it("does not fetch when next is disabled on the last page", () => {
    renderWithQuery(
      <ListingsTable
        listings={make(6)}
        total={42}
        pageSize={9}
        currentPage={5}
      />,
    );
    fireEvent.click(screen.getByRole("button", { name: /go to next page/i }));
    expect(fetchSpy).not.toHaveBeenCalled();
  });
});
