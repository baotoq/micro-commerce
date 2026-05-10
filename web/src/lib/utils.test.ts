import { describe, expect, it } from "vitest";
import { cn } from "@/lib/utils";

describe("cn", () => {
  it("returns empty string when called with no arguments", () => {
    expect(cn()).toBe("");
  });

  it("joins class names", () => {
    expect(cn("foo", "bar")).toBe("foo bar");
  });

  it("filters falsy values", () => {
    expect(cn("foo", undefined, null, false, "", "bar")).toBe("foo bar");
  });

  it("resolves tailwind merge conflicts — last wins", () => {
    expect(cn("p-2", "p-4")).toBe("p-4");
    expect(cn("text-red-500", "text-blue-500")).toBe("text-blue-500");
  });

  it("accepts conditional object inputs", () => {
    expect(cn({ "font-bold": true, italic: false })).toBe("font-bold");
  });

  it("accepts array inputs", () => {
    expect(cn(["flex", "items-center"])).toBe("flex items-center");
  });

  it("handles mixed array and string inputs", () => {
    expect(cn(["flex"], "gap-2", { "p-4": true })).toBe("flex gap-2 p-4");
  });

  it("resolves conflicting padding within an array", () => {
    expect(cn(["px-2", "px-4"])).toBe("px-4");
  });
});
