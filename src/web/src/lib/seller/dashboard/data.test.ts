// web/src/lib/seller/dashboard/data.test.ts
import { describe, expect, it } from "vitest";
import { getTodayItems } from "@/lib/seller/dashboard/data";

describe("seller dashboard data", () => {
  it("today items are non-empty", () => {
    expect(getTodayItems().length).toBeGreaterThan(0);
  });
});
