import { describe, expect, it } from "vitest";
import { pageHref, paginate } from "./pagination";

describe("paginate", () => {
  it("computes a 5-page split from 42 items at pageSize 9", () => {
    const v = paginate(1, 42, 9);
    expect(v.totalPages).toBe(5);
    expect(v.page).toBe(1);
    expect(v.hasPrev).toBe(false);
    expect(v.hasNext).toBe(true);
    expect(v.prevPage).toBeNull();
    expect(v.nextPage).toBe(2);
    expect(v.window).toEqual([1, 2, 3]);
  });

  it("centers the window around the current page in the middle", () => {
    expect(paginate(3, 42, 9).window).toEqual([2, 3, 4]);
  });

  it("clamps the window to the end of the range", () => {
    const v = paginate(5, 42, 9);
    expect(v.window).toEqual([3, 4, 5]);
    expect(v.hasNext).toBe(false);
    expect(v.nextPage).toBeNull();
  });

  it("clamps an out-of-range page to the last valid page", () => {
    const v = paginate(99, 42, 9);
    expect(v.page).toBe(5);
    expect(v.hasNext).toBe(false);
  });

  it("collapses to a single page when total fits in one page", () => {
    const v = paginate(1, 5, 9);
    expect(v.totalPages).toBe(1);
    expect(v.window).toEqual([1]);
    expect(v.hasPrev).toBe(false);
    expect(v.hasNext).toBe(false);
  });

  it("treats currentPage <= 0 as page 1", () => {
    expect(paginate(0, 42, 9).page).toBe(1);
    expect(paginate(-5, 42, 9).page).toBe(1);
  });

  it("respects a custom window size", () => {
    expect(paginate(3, 50, 5, 5).window).toEqual([1, 2, 3, 4, 5]);
  });
});

describe("pageHref", () => {
  it("omits the query for page 1", () => {
    expect(pageHref(1)).toBe("?");
  });

  it("emits ?page=N for later pages", () => {
    expect(pageHref(3)).toBe("?page=3");
  });
});
