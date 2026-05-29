import { describe, expect, it } from "vitest";
import { formatPromoWhat, formatPromoWindow } from "@/lib/seller/promos/format";

describe("formatPromoWhat", () => {
  it("formats percentage with no min order", () => {
    expect(
      formatPromoWhat({
        kind: "percentage",
        percentValue: 20,
        description: "sitewide",
      }),
    ).toBe("20% off · sitewide");
  });

  it("formats fixed amount with min order", () => {
    expect(
      formatPromoWhat({
        kind: "fixed",
        fixedAmount: 10,
        description: "first order",
        minOrderAmount: 40,
      }),
    ).toBe("$10 off · first order $40+");
  });

  it("formats percentage with no min order (followers only)", () => {
    expect(
      formatPromoWhat({
        kind: "percentage",
        percentValue: 15,
        description: "followers only",
      }),
    ).toBe("15% off · followers only");
  });

  it("formats fixed amount with no min order", () => {
    expect(
      formatPromoWhat({
        kind: "fixed",
        fixedAmount: 5,
        description: "sitewide",
      }),
    ).toBe("$5 off · sitewide");
  });

  it("formats percentage with min order appended", () => {
    expect(
      formatPromoWhat({
        kind: "percentage",
        percentValue: 10,
        description: "any order",
        minOrderAmount: 50,
      }),
    ).toBe("10% off · any order $50+");
  });

  it("ignores zero minOrderAmount", () => {
    expect(
      formatPromoWhat({
        kind: "percentage",
        percentValue: 10,
        description: "sitewide",
        minOrderAmount: 0,
      }),
    ).toBe("10% off · sitewide");
  });
});

describe("formatPromoWindow", () => {
  it("formats window with both dates", () => {
    expect(
      formatPromoWindow({
        startsAt: "2024-04-01T00:00:00Z",
        endsAt: "2024-05-15T00:00:00Z",
        status: "Active",
      }),
    ).toBe("Apr 1 → May 15");
  });

  it("returns 'Always' when no dates and status Active", () => {
    expect(formatPromoWindow({ status: "Active" })).toBe("Always");
  });

  it("returns 'Drafted' when no dates and status Draft", () => {
    expect(formatPromoWindow({ status: "Draft" })).toBe("Drafted");
  });

  it("returns 'Ended' when no dates and status Ended", () => {
    expect(formatPromoWindow({ status: "Ended" })).toBe("Ended");
  });

  it("formats window for Apr 22 → May 6", () => {
    expect(
      formatPromoWindow({
        startsAt: "2024-04-22T00:00:00Z",
        endsAt: "2024-05-06T00:00:00Z",
        status: "Active",
      }),
    ).toBe("Apr 22 → May 6");
  });
});
