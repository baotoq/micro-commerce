import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { PromosTable } from "@/components/seller/promos-table";
import type { PromoCode } from "@/lib/seller/promos/types";

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
});
