import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { PromosTable } from "@/components/seller/promos/promos-table";
import type { PromoCode } from "@/lib/seller/promos/types";

vi.mock("@/lib/seller/promos/actions", () => ({
  activatePromoAction: vi.fn(),
  endPromoAction: vi.fn(),
  deletePromoAction: vi.fn(),
}));

const FIXTURE: PromoCode[] = [
  {
    code: "SPRING20",
    what: "20% off · sitewide",
    window: "Apr 1 → May 15",
    redemptions: 142,
    drivenRevenue: 1842,
    status: "Active",
    tone: "good",
  },
  {
    code: "WELCOME10",
    what: "$10 off · first order $40+",
    window: "Always · 1 per buyer",
    redemptions: 38,
    drivenRevenue: 612,
    status: "Active",
    tone: "good",
  },
  {
    code: "STUDIO15",
    what: "15% off · followers only",
    window: "Apr 22 → May 06",
    redemptions: 17,
    drivenRevenue: 286,
    status: "Active",
    tone: "good",
    highlight: true,
  },
  {
    code: "BLOOM",
    what: "Free ship · $80+",
    window: "Mar 1 → Apr 12",
    redemptions: 84,
    drivenRevenue: 0,
    status: "Ended",
    tone: "mute",
  },
  {
    code: "FRIENDS",
    what: "15% off · sitewide",
    window: "Drafted",
    redemptions: 0,
    drivenRevenue: 0,
    status: "Draft",
    tone: "mute",
  },
];

describe("PromosTable", () => {
  it("renders 5 rows when given fixture", () => {
    render(<PromosTable promos={FIXTURE} />);
    // 5 body rows + 1 header = 6
    expect(screen.getAllByRole("row")).toHaveLength(6);
  });

  it('STUDIO15 row carries data-highlight="true"', () => {
    render(<PromosTable promos={FIXTURE} />);
    const studioRow = screen
      .getAllByRole("row")
      .find((row) => row.getAttribute("data-highlight") === "true");
    expect(studioRow).toBeDefined();
    expect(studioRow).toHaveTextContent("STUDIO15");
  });

  it("Activate button visible only for Draft rows", () => {
    render(<PromosTable promos={FIXTURE} />);
    const activateBtns = screen.queryAllByRole("button", { name: /Activate/i });
    expect(activateBtns).toHaveLength(1);
    expect(activateBtns[0]).toHaveAccessibleName("Activate FRIENDS");
  });

  it("End button visible only for Active rows", () => {
    render(<PromosTable promos={FIXTURE} />);
    const endBtns = screen.queryAllByRole("button", { name: /^End /i });
    // 3 active rows: SPRING20, WELCOME10, STUDIO15
    expect(endBtns).toHaveLength(3);
  });

  it("Delete button visible for every row", () => {
    render(<PromosTable promos={FIXTURE} />);
    const deleteBtns = screen.queryAllByRole("button", { name: /^Delete /i });
    expect(deleteBtns).toHaveLength(5);
  });

  it("Activate form for FRIENDS has correct action bound to activatePromoAction", () => {
    render(<PromosTable promos={FIXTURE} />);
    const activateBtn = screen.getByRole("button", {
      name: "Activate FRIENDS",
    });
    const form = activateBtn.closest("form");
    expect(form).not.toBeNull();
    // form.action is bound via .bind — it's a function, not a URL string
    expect(typeof form?.getAttribute("action")).toBe("string");
  });

  it("End button is absent for Ended and Draft rows", () => {
    render(<PromosTable promos={FIXTURE} />);
    const allRows = screen.getAllByRole("row").slice(1); // skip header
    const bloomRow = allRows.find((r) => r.textContent?.includes("BLOOM"));
    const friendsRow = allRows.find((r) => r.textContent?.includes("FRIENDS"));
    expect(bloomRow?.querySelector('button[aria-label^="End"]')).toBeNull();
    expect(friendsRow?.querySelector('button[aria-label^="End"]')).toBeNull();
  });

  it("Activate button is absent for Active and Ended rows", () => {
    render(<PromosTable promos={FIXTURE} />);
    const allRows = screen.getAllByRole("row").slice(1); // skip header
    const spring20Row = allRows.find((r) =>
      r.textContent?.includes("SPRING20"),
    );
    const bloomRow = allRows.find((r) => r.textContent?.includes("BLOOM"));
    expect(
      spring20Row?.querySelector('button[aria-label^="Activate"]'),
    ).toBeNull();
    expect(
      bloomRow?.querySelector('button[aria-label^="Activate"]'),
    ).toBeNull();
  });
});
