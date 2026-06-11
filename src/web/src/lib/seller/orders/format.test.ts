import { describe, expect, it } from "vitest";
import { DEMO_NOW } from "@/lib/seller/demo-clock";
import {
  formatAge,
  formatPlacedLabel,
  formatRelative,
  shortName,
  shortNamePanel,
  statusToDisplay,
} from "@/lib/seller/orders/format";

// Canonical demo instant: 2026-04-08T16:45:00-07:00 (America/Los_Angeles, PDT).
// All seeded order timestamps below are anchored to this instant so that the
// relative-time labels match the hi-fi mock verbatim.

describe("formatPlacedLabel — the 10 inbox order timestamps", () => {
  const cases: Array<[string, string, string]> = [
    ["#1042", "2026-04-08T21:14:00Z", "Today · 2:14 PM"],
    ["#1041", "2026-04-08T18:08:00Z", "Today · 11:08 AM"],
    ["#1040", "2026-04-08T16:41:00Z", "Today · 9:41 AM"],
    ["#1039", "2026-04-07T21:30:00Z", "Yesterday"],
    ["#1038", "2026-04-06T17:00:00Z", "2 days ago"],
    ["#1037", "2026-04-05T16:00:00Z", "3 days ago"],
    ["#1036", "2026-04-04T18:00:00Z", "4 days ago"],
    ["#1035", "2026-04-03T15:00:00Z", "5 days ago"],
    ["#1034", "2026-04-02T22:00:00Z", "6 days ago"],
    ["#1033", "2026-04-01T16:00:00Z", "1 week ago"],
  ];

  for (const [id, iso, expected] of cases) {
    it(`${id} → "${expected}"`, () => {
      expect(formatPlacedLabel(new Date(iso), DEMO_NOW)).toBe(expected);
    });
  }
});

describe("formatAge — inbox row age chips", () => {
  it('#1042 (2h ago) → "2h"', () => {
    expect(formatAge(new Date("2026-04-08T21:14:00Z"), DEMO_NOW)).toBe("2h");
  });

  it('#1041 (5h ago) → "5h"', () => {
    expect(formatAge(new Date("2026-04-08T18:08:00Z"), DEMO_NOW)).toBe("5h");
  });

  it('#1040 (7h ago) → "7h"', () => {
    expect(formatAge(new Date("2026-04-08T16:41:00Z"), DEMO_NOW)).toBe("7h");
  });

  it('#1039 (yesterday) → "1d"', () => {
    expect(formatAge(new Date("2026-04-07T21:30:00Z"), DEMO_NOW)).toBe("1d");
  });

  it('#1038 (2 days) → "2d"', () => {
    expect(formatAge(new Date("2026-04-06T17:00:00Z"), DEMO_NOW)).toBe("2d");
  });

  it('#1033 (1 week) → "7d"', () => {
    expect(formatAge(new Date("2026-04-01T16:00:00Z"), DEMO_NOW)).toBe("7d");
  });
});

describe("formatRelative — the 5 timeline whens (relative to PlacedAt #1042)", () => {
  const placedAt = new Date("2026-04-08T21:14:00Z");
  const minus = (mins: number) => new Date(placedAt.getTime() - mins * 60_000);

  it('PlacedAt − 2h → "2h ago"', () => {
    expect(formatRelative(minus(120), placedAt)).toBe("2h ago");
  });

  it('PlacedAt − 1h → "1h ago"', () => {
    expect(formatRelative(minus(60), placedAt)).toBe("1h ago");
  });

  it('PlacedAt − 52m → "52m ago"', () => {
    expect(formatRelative(minus(52), placedAt)).toBe("52m ago");
  });

  it('PlacedAt − 14m → "14m ago"', () => {
    expect(formatRelative(minus(14), placedAt)).toBe("14m ago");
  });

  it('PlacedAt − 8m → "8m ago"', () => {
    expect(formatRelative(minus(8), placedAt)).toBe("8m ago");
  });
});

describe("statusToDisplay — wire key → label + tone", () => {
  it("new → { New, warn }", () => {
    expect(statusToDisplay("new")).toEqual({ label: "New", tone: "warn" });
  });

  it("packed → { Packed, mute }", () => {
    expect(statusToDisplay("packed")).toEqual({
      label: "Packed",
      tone: "mute",
    });
  });

  it("shipped → { Shipped, mute }", () => {
    expect(statusToDisplay("shipped")).toEqual({
      label: "Shipped",
      tone: "mute",
    });
  });

  it("delivered → { Delivered, good }", () => {
    expect(statusToDisplay("delivered")).toEqual({
      label: "Delivered",
      tone: "good",
    });
  });

  it("refund-requested → { Refund req., bad }", () => {
    expect(statusToDisplay("refund-requested")).toEqual({
      label: "Refund req.",
      tone: "bad",
    });
  });

  it("cancelled → { Cancelled, mute }", () => {
    expect(statusToDisplay("cancelled")).toEqual({
      label: "Cancelled",
      tone: "mute",
    });
  });
});

describe("shortName / shortNamePanel", () => {
  it('shortName("Sasha Leblanc") → "Sasha L."', () => {
    expect(shortName("Sasha Leblanc")).toBe("Sasha L.");
  });

  it('shortNamePanel("Sasha Leblanc") → "Sasha L"', () => {
    expect(shortNamePanel("Sasha Leblanc")).toBe("Sasha L");
  });
});
