import { describe, expect, it } from "vitest";
import { promotionInputSchema } from "@/lib/seller/promos/schema";

const basePercentage = {
  kind: "percentage" as const,
  code: "SPRING20",
  description: "sitewide",
  percentValue: 20,
};

const baseFixed = {
  kind: "fixed" as const,
  code: "WELCOME10",
  description: "first order",
  fixedAmount: 10,
};

describe("promotionInputSchema — discriminator", () => {
  it("accepts valid percentage input", () => {
    const result = promotionInputSchema.safeParse(basePercentage);
    expect(result.success).toBe(true);
  });

  it("accepts valid fixed input", () => {
    const result = promotionInputSchema.safeParse(baseFixed);
    expect(result.success).toBe(true);
  });

  it("rejects unknown kind", () => {
    const result = promotionInputSchema.safeParse({
      ...basePercentage,
      kind: "freeshipping",
    });
    expect(result.success).toBe(false);
  });

  it("rejects percentage without percentValue", () => {
    const { percentValue: _, ...withoutValue } = basePercentage;
    const result = promotionInputSchema.safeParse(withoutValue);
    expect(result.success).toBe(false);
  });

  it("rejects fixed without fixedAmount", () => {
    const { fixedAmount: _, ...withoutAmount } = baseFixed;
    const result = promotionInputSchema.safeParse(withoutAmount);
    expect(result.success).toBe(false);
  });
});

describe("promotionInputSchema — percentage range [1,100]", () => {
  it("rejects percentValue of 0", () => {
    const result = promotionInputSchema.safeParse({
      ...basePercentage,
      percentValue: 0,
    });
    expect(result.success).toBe(false);
  });

  it("rejects percentValue of 101", () => {
    const result = promotionInputSchema.safeParse({
      ...basePercentage,
      percentValue: 101,
    });
    expect(result.success).toBe(false);
  });

  it("accepts percentValue of 1", () => {
    const result = promotionInputSchema.safeParse({
      ...basePercentage,
      percentValue: 1,
    });
    expect(result.success).toBe(true);
  });

  it("accepts percentValue of 100", () => {
    const result = promotionInputSchema.safeParse({
      ...basePercentage,
      percentValue: 100,
    });
    expect(result.success).toBe(true);
  });

  it("rejects non-integer percentValue", () => {
    const result = promotionInputSchema.safeParse({
      ...basePercentage,
      percentValue: 10.5,
    });
    expect(result.success).toBe(false);
  });
});

describe("promotionInputSchema — fixed amount > 0", () => {
  it("rejects fixedAmount of 0", () => {
    const result = promotionInputSchema.safeParse({
      ...baseFixed,
      fixedAmount: 0,
    });
    expect(result.success).toBe(false);
  });

  it("rejects negative fixedAmount", () => {
    const result = promotionInputSchema.safeParse({
      ...baseFixed,
      fixedAmount: -5,
    });
    expect(result.success).toBe(false);
  });

  it("accepts positive fixedAmount", () => {
    const result = promotionInputSchema.safeParse({
      ...baseFixed,
      fixedAmount: 0.01,
    });
    expect(result.success).toBe(true);
  });
});

describe("promotionInputSchema — startsAt < endsAt refine", () => {
  it("accepts valid window where startsAt < endsAt", () => {
    const result = promotionInputSchema.safeParse({
      ...basePercentage,
      startsAt: "2024-04-01T00:00:00Z",
      endsAt: "2024-05-15T00:00:00Z",
    });
    expect(result.success).toBe(true);
  });

  it("rejects when startsAt equals endsAt", () => {
    const result = promotionInputSchema.safeParse({
      ...basePercentage,
      startsAt: "2024-04-01T00:00:00Z",
      endsAt: "2024-04-01T00:00:00Z",
    });
    expect(result.success).toBe(false);
  });

  it("rejects when startsAt is after endsAt", () => {
    const result = promotionInputSchema.safeParse({
      ...basePercentage,
      startsAt: "2024-05-01T00:00:00Z",
      endsAt: "2024-04-01T00:00:00Z",
    });
    expect(result.success).toBe(false);
  });

  it("accepts with only startsAt (no endsAt)", () => {
    const result = promotionInputSchema.safeParse({
      ...basePercentage,
      startsAt: "2024-04-01T00:00:00Z",
    });
    expect(result.success).toBe(true);
  });

  it("accepts with only endsAt (no startsAt)", () => {
    const result = promotionInputSchema.safeParse({
      ...basePercentage,
      endsAt: "2024-05-01T00:00:00Z",
    });
    expect(result.success).toBe(true);
  });

  it("accepts with no dates at all", () => {
    const result = promotionInputSchema.safeParse(basePercentage);
    expect(result.success).toBe(true);
  });
});

describe("promotionInputSchema — FormData coercion", () => {
  it("coerces percentValue from string", () => {
    const result = promotionInputSchema.safeParse({
      ...basePercentage,
      percentValue: "20",
    });
    expect(result.success).toBe(true);
    if (result.success && result.data.kind === "percentage") {
      expect(result.data.percentValue).toBe(20);
    }
  });

  it("coerces fixedAmount from string", () => {
    const result = promotionInputSchema.safeParse({
      ...baseFixed,
      fixedAmount: "10.5",
    });
    expect(result.success).toBe(true);
    if (result.success && result.data.kind === "fixed") {
      expect(result.data.fixedAmount).toBe(10.5);
    }
  });

  it("coerces activateImmediately from string 'true'", () => {
    const result = promotionInputSchema.safeParse({
      ...basePercentage,
      activateImmediately: "true",
    });
    expect(result.success).toBe(true);
    if (result.success) {
      expect(result.data.activateImmediately).toBe(true);
    }
  });

  it("defaults activateImmediately to false when absent", () => {
    const result = promotionInputSchema.safeParse(basePercentage);
    expect(result.success).toBe(true);
    if (result.success) {
      expect(result.data.activateImmediately).toBe(false);
    }
  });
});

describe("promotionInputSchema — code and description validation", () => {
  it("rejects empty code", () => {
    const result = promotionInputSchema.safeParse({
      ...basePercentage,
      code: "",
    });
    expect(result.success).toBe(false);
  });

  it("rejects code longer than 40 chars", () => {
    const result = promotionInputSchema.safeParse({
      ...basePercentage,
      code: "A".repeat(41),
    });
    expect(result.success).toBe(false);
  });

  it("rejects empty description", () => {
    const result = promotionInputSchema.safeParse({
      ...basePercentage,
      description: "",
    });
    expect(result.success).toBe(false);
  });

  it("rejects description longer than 200 chars", () => {
    const result = promotionInputSchema.safeParse({
      ...basePercentage,
      description: "x".repeat(201),
    });
    expect(result.success).toBe(false);
  });

  it("trims whitespace from code", () => {
    const result = promotionInputSchema.safeParse({
      ...basePercentage,
      code: "  SPRING20  ",
    });
    expect(result.success).toBe(true);
    if (result.success) {
      expect(result.data.code).toBe("SPRING20");
    }
  });
});
