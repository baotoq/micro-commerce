import { afterEach, describe, expect, it, vi } from "vitest";

vi.mock("server-only", () => ({}));
vi.mock("next/cache", () => ({ updateTag: vi.fn() }));
vi.mock("@/lib/catalog/promotions", () => ({
  createPromotion: vi.fn(),
  updatePromotion: vi.fn(),
  activatePromotion: vi.fn(),
  endPromotion: vi.fn(),
  deletePromotion: vi.fn(),
}));

import { updateTag } from "next/cache";
import * as promotionsClient from "@/lib/catalog/promotions";
import type { PromotionDto } from "@/lib/seller/promos/types";
import {
  activatePromoAction,
  createPromoAction,
  deletePromoAction,
  endPromoAction,
  updatePromoAction,
} from "./actions";

function makeFormData(data: Record<string, string | string[]>): FormData {
  const fd = new FormData();
  for (const [k, v] of Object.entries(data)) {
    if (Array.isArray(v)) {
      for (const item of v) fd.append(k, item);
    } else {
      fd.append(k, v);
    }
  }
  return fd;
}

const validPercentageFormData: Record<string, string> = {
  kind: "percentage",
  code: "SPRING20",
  description: "sitewide",
  percentValue: "20",
  activateImmediately: "false",
};

const validFixedFormData: Record<string, string> = {
  kind: "fixed",
  code: "WELCOME10",
  description: "first order",
  fixedAmount: "10",
  minOrderAmount: "40",
  activateImmediately: "true",
};

const mockDto: PromotionDto = {
  code: "SPRING20",
  description: "sitewide",
  kind: "percentage",
  percentValue: 20,
  fixedAmount: null,
  minOrderAmount: null,
  startsAt: null,
  endsAt: null,
  status: "active",
};

describe("createPromoAction", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("creates a promotion and returns ok:true with code", async () => {
    vi.mocked(promotionsClient.createPromotion).mockResolvedValueOnce(mockDto);

    const result = await createPromoAction(
      makeFormData(validPercentageFormData),
    );

    expect(result.ok).toBe(true);
    if (result.ok) expect(result.sku).toBe("SPRING20");
    expect(promotionsClient.createPromotion).toHaveBeenCalledWith(
      expect.objectContaining({
        code: "SPRING20",
        description: "sitewide",
        kind: "percentage",
        percentValue: 20,
        activateImmediately: false,
      }),
    );
    expect(updateTag).toHaveBeenCalledWith("promotions");
  });

  it("creates with activateImmediately=true from FormData", async () => {
    vi.mocked(promotionsClient.createPromotion).mockResolvedValueOnce({
      ...mockDto,
      code: "WELCOME10",
      kind: "fixed",
      fixedAmount: 10,
      percentValue: null,
      status: "active",
    });

    const result = await createPromoAction(makeFormData(validFixedFormData));

    expect(result.ok).toBe(true);
    expect(promotionsClient.createPromotion).toHaveBeenCalledWith(
      expect.objectContaining({ activateImmediately: true }),
    );
  });

  it("returns fieldErrors when input is invalid (missing required field)", async () => {
    const result = await createPromoAction(
      makeFormData({
        kind: "percentage",
        code: "SPRING20",
        description: "sitewide",
      }),
    );

    expect(result.ok).toBe(false);
    if (!result.ok) {
      expect(result.fieldErrors).toBeDefined();
    }
    expect(promotionsClient.createPromotion).not.toHaveBeenCalled();
  });

  it("returns validation error round-trip with values", async () => {
    const result = await createPromoAction(
      makeFormData({ ...validPercentageFormData, percentValue: "-1" }),
    );

    expect(result.ok).toBe(false);
    if (!result.ok) {
      expect(result.fieldErrors?.percentValue).toBeDefined();
      expect(result.values?.code).toBe("SPRING20");
    }
  });

  it("returns ok:false with duplicate-code message on PROMOTION_CODE_DUPLICATE", async () => {
    vi.mocked(promotionsClient.createPromotion).mockRejectedValueOnce(
      new Error("PROMOTION_CODE_DUPLICATE"),
    );

    const result = await createPromoAction(
      makeFormData(validPercentageFormData),
    );

    expect(result.ok).toBe(false);
    if (!result.ok) expect(result.error).toMatch(/already exists/i);
    expect(updateTag).not.toHaveBeenCalled();
  });
});

describe("updatePromoAction", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("updates a promotion and returns ok:true with code", async () => {
    vi.mocked(promotionsClient.updatePromotion).mockResolvedValueOnce(mockDto);

    const result = await updatePromoAction(
      "SPRING20",
      makeFormData(validPercentageFormData),
    );

    expect(result.ok).toBe(true);
    if (result.ok) expect(result.sku).toBe("SPRING20");
    expect(updateTag).toHaveBeenCalledWith("promotions");
  });

  it("returns ok:false with friendly error on 404 (null response)", async () => {
    vi.mocked(promotionsClient.updatePromotion).mockResolvedValueOnce(null);

    const result = await updatePromoAction(
      "MISSING",
      makeFormData(validPercentageFormData),
    );

    expect(result.ok).toBe(false);
    if (!result.ok) expect(result.error).toMatch(/not found/i);
  });

  it("returns fieldErrors when input is invalid", async () => {
    const result = await updatePromoAction(
      "SPRING20",
      makeFormData({ ...validPercentageFormData, percentValue: "101" }),
    );

    expect(result.ok).toBe(false);
    if (!result.ok) expect(result.fieldErrors?.percentValue).toBeDefined();
    expect(promotionsClient.updatePromotion).not.toHaveBeenCalled();
  });
});

describe("activatePromoAction", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("activates a promotion and returns ok:true", async () => {
    vi.mocked(promotionsClient.activatePromotion).mockResolvedValueOnce(
      mockDto,
    );

    const result = await activatePromoAction("SPRING20");

    expect(result.ok).toBe(true);
    if (result.ok) expect(result.sku).toBe("SPRING20");
    expect(updateTag).toHaveBeenCalledWith("promotions");
  });

  it("returns ok:false when promo not found (null response)", async () => {
    vi.mocked(promotionsClient.activatePromotion).mockResolvedValueOnce(null);

    const result = await activatePromoAction("MISSING");

    expect(result.ok).toBe(false);
    if (!result.ok) expect(result.error).toMatch(/not found/i);
  });

  it("returns ok:false on invalid transition", async () => {
    vi.mocked(promotionsClient.activatePromotion).mockRejectedValueOnce(
      new Error("PROMOTION_INVALID_TRANSITION"),
    );

    const result = await activatePromoAction("SPRING20");

    expect(result.ok).toBe(false);
  });
});

describe("endPromoAction", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("ends a promotion and returns ok:true", async () => {
    vi.mocked(promotionsClient.endPromotion).mockResolvedValueOnce({
      ...mockDto,
      status: "ended",
    });

    const result = await endPromoAction("SPRING20");

    expect(result.ok).toBe(true);
    if (result.ok) expect(result.sku).toBe("SPRING20");
    expect(updateTag).toHaveBeenCalledWith("promotions");
  });

  it("returns ok:false when promo not found (null response)", async () => {
    vi.mocked(promotionsClient.endPromotion).mockResolvedValueOnce(null);

    const result = await endPromoAction("MISSING");

    expect(result.ok).toBe(false);
    if (!result.ok) expect(result.error).toMatch(/not found/i);
  });
});

describe("deletePromoAction", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("deletes a promotion and returns ok:true", async () => {
    vi.mocked(promotionsClient.deletePromotion).mockResolvedValueOnce(true);

    const result = await deletePromoAction("SPRING20");

    expect(result.ok).toBe(true);
    if (result.ok) expect(result.sku).toBe("SPRING20");
    expect(promotionsClient.deletePromotion).toHaveBeenCalledWith("SPRING20");
    expect(updateTag).toHaveBeenCalledWith("promotions");
  });

  it("returns ok:false with friendly error when not found", async () => {
    vi.mocked(promotionsClient.deletePromotion).mockResolvedValueOnce(false);

    const result = await deletePromoAction("MISSING");

    expect(result.ok).toBe(false);
    if (!result.ok) expect(result.error).toMatch(/not found/i);
  });
});
