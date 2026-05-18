// web/src/lib/seller/application/data.test.ts
import { describe, expect, it } from "vitest";
import {
  getLaunchChecklist,
  getSetupSteps,
} from "@/lib/seller/application/data";

describe("seller application data", () => {
  it("setup steps include done, active, and pending states", () => {
    const steps = getSetupSteps();
    expect(steps.length).toBeGreaterThan(0);
    const statuses = steps.map((s) => s.status);
    expect(statuses).toContain("done");
    expect(statuses).toContain("active");
    expect(statuses).toContain("pending");
  });

  it("launch checklist has at least one done and one undone task", () => {
    const tasks = getLaunchChecklist();
    expect(tasks.some((t) => t.done)).toBe(true);
    expect(tasks.some((t) => !t.done)).toBe(true);
  });
});
