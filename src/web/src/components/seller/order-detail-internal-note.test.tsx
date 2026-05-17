import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { OrderDetailInternalNote } from "./order-detail-internal-note";

const TAGS = ["priority", "fragile", "gift"];
const ACTIVE_TAGS = ["priority"];

describe("OrderDetailInternalNote", () => {
  it("renders the 'Internal note' heading", () => {
    render(
      <OrderDetailInternalNote
        note="Test note"
        tags={TAGS}
        activeTags={ACTIVE_TAGS}
      />,
    );
    expect(screen.getByText("Internal note")).toBeInTheDocument();
  });

  it("renders the note text", () => {
    render(
      <OrderDetailInternalNote
        note="Pack carefully — fragile items inside."
        tags={TAGS}
        activeTags={ACTIVE_TAGS}
      />,
    );
    expect(
      screen.getByText("Pack carefully — fragile items inside."),
    ).toBeInTheDocument();
  });

  it("renders an empty note without errors", () => {
    render(
      <OrderDetailInternalNote note="" tags={TAGS} activeTags={ACTIVE_TAGS} />,
    );
    expect(screen.getByText("Internal note")).toBeInTheDocument();
  });

  it("renders all tags", () => {
    render(
      <OrderDetailInternalNote
        note="note"
        tags={TAGS}
        activeTags={ACTIVE_TAGS}
      />,
    );
    expect(screen.getByText("priority")).toBeInTheDocument();
    expect(screen.getByText("fragile")).toBeInTheDocument();
    expect(screen.getByText("gift")).toBeInTheDocument();
  });

  it("renders active tag with filled style", () => {
    render(
      <OrderDetailInternalNote
        note="note"
        tags={TAGS}
        activeTags={["priority"]}
      />,
    );
    const activeTag = screen.getByText("priority");
    expect(activeTag.className).toContain("bg-foreground");
    expect(activeTag.className).toContain("text-white");
  });

  it("renders inactive tags with outline style", () => {
    render(
      <OrderDetailInternalNote
        note="note"
        tags={TAGS}
        activeTags={["priority"]}
      />,
    );
    const fragileTag = screen.getByText("fragile");
    expect(fragileTag.className).toContain("border");
    expect(fragileTag.className).not.toContain("bg-foreground");
  });

  it("renders no tags when tags array is empty", () => {
    render(<OrderDetailInternalNote note="note" tags={[]} activeTags={[]} />);
    expect(
      screen.queryByRole("generic", { name: /tag/i }),
    ).not.toBeInTheDocument();
    expect(screen.getByText("note")).toBeInTheDocument();
  });

  it("renders all tags as inactive when activeTags is empty", () => {
    render(<OrderDetailInternalNote note="note" tags={TAGS} activeTags={[]} />);
    for (const tag of TAGS) {
      const el = screen.getByText(tag);
      expect(el.className).not.toContain("bg-foreground");
      expect(el.className).toContain("border");
    }
  });

  it("renders all tags as active when all are in activeTags", () => {
    render(
      <OrderDetailInternalNote note="note" tags={TAGS} activeTags={TAGS} />,
    );
    for (const tag of TAGS) {
      const el = screen.getByText(tag);
      expect(el.className).toContain("bg-foreground");
      expect(el.className).toContain("text-white");
    }
  });
});
