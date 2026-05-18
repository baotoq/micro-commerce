import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { RangeTabs } from "@/components/seller/shared/range-tabs";

const OPTIONS = ["7d", "30d", "90d", "1y"];

describe("RangeTabs", () => {
  it("renders all option buttons", () => {
    render(<RangeTabs options={OPTIONS} defaultValue="30d" />);
    for (const opt of OPTIONS) {
      expect(screen.getByRole("button", { name: opt })).toBeInTheDocument();
    }
  });

  it("marks defaultValue as pressed initially", () => {
    render(<RangeTabs options={OPTIONS} defaultValue="30d" />);
    expect(screen.getByRole("button", { name: "30d" })).toHaveAttribute(
      "aria-pressed",
      "true",
    );
  });

  it("marks other options as not pressed initially", () => {
    render(<RangeTabs options={OPTIONS} defaultValue="30d" />);
    for (const opt of OPTIONS.filter((o) => o !== "30d")) {
      expect(screen.getByRole("button", { name: opt })).toHaveAttribute(
        "aria-pressed",
        "false",
      );
    }
  });

  it("switches active tab on click", () => {
    render(<RangeTabs options={OPTIONS} defaultValue="30d" />);
    fireEvent.click(screen.getByRole("button", { name: "90d" }));
    expect(screen.getByRole("button", { name: "90d" })).toHaveAttribute(
      "aria-pressed",
      "true",
    );
    expect(screen.getByRole("button", { name: "30d" })).toHaveAttribute(
      "aria-pressed",
      "false",
    );
  });

  it("only one tab is pressed at a time after multiple clicks", () => {
    render(<RangeTabs options={OPTIONS} defaultValue="7d" />);
    fireEvent.click(screen.getByRole("button", { name: "1y" }));
    const pressed = OPTIONS.filter(
      (opt) =>
        screen
          .getByRole("button", { name: opt })
          .getAttribute("aria-pressed") === "true",
    );
    expect(pressed).toHaveLength(1);
    expect(pressed[0]).toBe("1y");
  });
});
