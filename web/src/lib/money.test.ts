import { describe, expect, it } from "vitest";
import { money } from "@/lib/money";

describe("money", () => {
  it("formats whole numbers with default $ prefix and 2 decimals", () => {
    expect(money(86)).toBe("$86.00");
    expect(money(0)).toBe("$0.00");
  });

  it("inserts thousands separators", () => {
    expect(money(1000)).toBe("$1,000.00");
    expect(money(1234567)).toBe("$1,234,567.00");
  });

  it("rounds to two decimals", () => {
    expect(money(1.005)).toBe("$1.00");
    expect(money(1.006)).toBe("$1.01");
    expect(money(99.999)).toBe("$100.00");
  });

  it("supports custom currency prefixes", () => {
    expect(money(50, "€")).toBe("€50.00");
    expect(money(1500, "")).toBe("1,500.00");
  });

  it("preserves negative sign", () => {
    expect(money(-1234.5)).toBe("$-1,234.50");
  });

  it("formats negative zero as positive zero", () => {
    expect(money(-0)).toBe("$0.00");
  });

  it("formats large numbers correctly", () => {
    expect(money(1000000)).toBe("$1,000,000.00");
    expect(money(9999999.99)).toBe("$9,999,999.99");
  });

  it("formats very small positive decimals", () => {
    expect(money(0.01)).toBe("$0.01");
    expect(money(0.001)).toBe("$0.00");
  });

  it("formats negative values with thousands separators", () => {
    expect(money(-1000000, "€")).toBe("€-1,000,000.00");
  });

  it("uses empty string currency prefix", () => {
    expect(money(0, "")).toBe("0.00");
    expect(money(42.5, "")).toBe("42.50");
  });
});
