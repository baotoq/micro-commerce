import { describe, expect, it } from "vitest";
import { computeListingHealth } from "./health";

const empty = {
  sku: "",
  name: "",
  category: "",
  price: "",
  inventory: "",
  status: "draft" as const,
  description: "",
  tags: [] as string[],
  weight: "",
  origin: "",
  photoUrls: [] as string[],
};

const fullySatisfied = {
  sku: "MC-VS-001",
  name: "Persimmon vase",
  category: "Vessels",
  price: "86",
  inventory: "24",
  status: "active" as const,
  description: "x".repeat(120),
  tags: ["ceramic", "minimal", "stoneware"],
  weight: "1.25",
  origin: "Portland, OR",
  photoUrls: ["https://x/1.jpg", "https://x/2.jpg", "https://x/3.jpg"],
};

describe("computeListingHealth", () => {
  it("returns 0 for an empty form", () => {
    expect(computeListingHealth(empty).score).toBe(0);
  });

  it("returns reasons for every missing rule when empty", () => {
    const { reasons } = computeListingHealth(empty);
    expect(reasons).toContain("Add a longer name");
    expect(reasons).toContain("Add a description over 100 chars");
    expect(reasons).toContain("Add 3+ photos");
    expect(reasons).toContain("Add 3+ tags");
    expect(reasons).toContain("Set a price");
    expect(reasons).toContain("Add inventory");
    expect(reasons).toContain("Choose a category");
    expect(reasons).toContain("Set shipping weight");
    expect(reasons).toContain("Add origin (City, ST)");
  });

  it("returns 100 when all rules are satisfied", () => {
    expect(computeListingHealth(fullySatisfied).score).toBe(100);
  });

  it("returns no reasons when fully satisfied", () => {
    expect(computeListingHealth(fullySatisfied).reasons).toHaveLength(0);
  });

  it("awards 15 for name length >= 3 only", () => {
    expect(computeListingHealth({ ...empty, name: "ab" }).score).toBe(0);
    expect(computeListingHealth({ ...empty, name: "abc" }).score).toBe(15);
  });

  it("awards 15 for description length >= 100", () => {
    expect(
      computeListingHealth({ ...empty, description: "x".repeat(99) }).score,
    ).toBe(0);
    expect(
      computeListingHealth({ ...empty, description: "x".repeat(100) }).score,
    ).toBe(15);
  });

  it("awards 5 points per photo up to 3 (capped at 20 for 3+)", () => {
    expect(
      computeListingHealth({ ...empty, photoUrls: ["a"] }).score,
    ).toBe(5);
    expect(
      computeListingHealth({ ...empty, photoUrls: ["a", "b"] }).score,
    ).toBe(10);
    expect(
      computeListingHealth({ ...empty, photoUrls: ["a", "b", "c"] }).score,
    ).toBe(20);
    expect(
      computeListingHealth({
        ...empty,
        photoUrls: ["a", "b", "c", "d", "e", "f"],
      }).score,
    ).toBe(20);
  });

  it("awards 10 for tags length >= 3", () => {
    expect(
      computeListingHealth({ ...empty, tags: ["a", "b"] }).score,
    ).toBe(0);
    expect(
      computeListingHealth({ ...empty, tags: ["a", "b", "c"] }).score,
    ).toBe(10);
  });

  it("awards 10 for price > 0", () => {
    expect(computeListingHealth({ ...empty, price: "0" }).score).toBe(0);
    expect(computeListingHealth({ ...empty, price: "0.01" }).score).toBe(10);
  });

  it("awards 10 for inventory >= 1", () => {
    expect(computeListingHealth({ ...empty, inventory: "0" }).score).toBe(0);
    expect(computeListingHealth({ ...empty, inventory: "1" }).score).toBe(10);
  });

  it("awards 10 for non-empty category", () => {
    expect(computeListingHealth({ ...empty, category: "" }).score).toBe(0);
    expect(computeListingHealth({ ...empty, category: "Vessels" }).score).toBe(
      10,
    );
  });

  it("awards 5 for weight > 0", () => {
    expect(computeListingHealth({ ...empty, weight: "0" }).score).toBe(0);
    expect(computeListingHealth({ ...empty, weight: "0.1" }).score).toBe(5);
  });

  it("awards 5 for origin matching City, ST format", () => {
    expect(
      computeListingHealth({ ...empty, origin: "Portland" }).score,
    ).toBe(0);
    expect(
      computeListingHealth({ ...empty, origin: "Portland, OR" }).score,
    ).toBe(5);
  });

  it("clamps score to 100", () => {
    const { score } = computeListingHealth(fullySatisfied);
    expect(score).toBeLessThanOrEqual(100);
    expect(score).toBeGreaterThanOrEqual(0);
  });

  it("is pure — same input yields same output and no mutation", () => {
    const snapshot = JSON.stringify(fullySatisfied);
    computeListingHealth(fullySatisfied);
    expect(JSON.stringify(fullySatisfied)).toBe(snapshot);
  });

  it("accepts numeric price/inventory/weight as well as strings", () => {
    const { score } = computeListingHealth({
      ...empty,
      price: 10 as unknown as string,
      inventory: 5 as unknown as string,
      weight: 1.5 as unknown as string,
    });
    expect(score).toBe(25);
  });
});
