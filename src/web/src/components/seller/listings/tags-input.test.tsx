import { fireEvent, render, screen } from "@testing-library/react";
import { useState } from "react";
import { describe, expect, it, vi } from "vitest";
import { TagsInput } from "./tags-input";

function Harness({
  initial = [] as string[],
  onChange,
}: {
  initial?: string[];
  onChange?: (v: string[]) => void;
}) {
  const [value, setValue] = useState<string[]>(initial);
  return (
    <TagsInput
      value={value}
      onChange={(v) => {
        setValue(v);
        onChange?.(v);
      }}
    />
  );
}

describe("TagsInput", () => {
  it("renders existing tags as chips", () => {
    render(<Harness initial={["ceramic", "minimal"]} />);
    expect(screen.getByText("ceramic")).toBeInTheDocument();
    expect(screen.getByText("minimal")).toBeInTheDocument();
  });

  it("commits a tag on Enter", () => {
    const onChange = vi.fn();
    render(<Harness onChange={onChange} />);
    const input = screen.getByRole("textbox") as HTMLInputElement;
    fireEvent.change(input, { target: { value: "stoneware" } });
    fireEvent.keyDown(input, { key: "Enter" });
    expect(onChange).toHaveBeenLastCalledWith(["stoneware"]);
    expect(input.value).toBe("");
  });

  it("commits a tag on comma", () => {
    const onChange = vi.fn();
    render(<Harness onChange={onChange} />);
    const input = screen.getByRole("textbox") as HTMLInputElement;
    fireEvent.change(input, { target: { value: "small-batch," } });
    expect(onChange).toHaveBeenLastCalledWith(["small-batch"]);
    expect(input.value).toBe("");
  });

  it("lowercases and trims new tags before committing", () => {
    const onChange = vi.fn();
    render(<Harness onChange={onChange} />);
    const input = screen.getByRole("textbox") as HTMLInputElement;
    fireEvent.change(input, { target: { value: "  Ceramic  " } });
    fireEvent.keyDown(input, { key: "Enter" });
    expect(onChange).toHaveBeenLastCalledWith(["ceramic"]);
  });

  it("dedupes when committing a tag that already exists", () => {
    const onChange = vi.fn();
    render(<Harness initial={["ceramic"]} onChange={onChange} />);
    const input = screen.getByRole("textbox") as HTMLInputElement;
    fireEvent.change(input, { target: { value: "ceramic" } });
    fireEvent.keyDown(input, { key: "Enter" });
    expect(onChange).not.toHaveBeenCalled();
    expect(input.value).toBe("");
  });

  it("removes last tag on Backspace when input is empty", () => {
    const onChange = vi.fn();
    render(<Harness initial={["a1", "b2"]} onChange={onChange} />);
    const input = screen.getByRole("textbox") as HTMLInputElement;
    fireEvent.keyDown(input, { key: "Backspace" });
    expect(onChange).toHaveBeenLastCalledWith(["a1"]);
  });

  it("does NOT remove tag on Backspace when input has text", () => {
    const onChange = vi.fn();
    render(<Harness initial={["a1", "b2"]} onChange={onChange} />);
    const input = screen.getByRole("textbox") as HTMLInputElement;
    fireEvent.change(input, { target: { value: "x" } });
    fireEvent.keyDown(input, { key: "Backspace" });
    // change handler ran, but onChange to the parent (tag list) didn't.
    expect(onChange).not.toHaveBeenCalled();
  });

  it("removes a tag when its chip × button is clicked", () => {
    const onChange = vi.fn();
    render(<Harness initial={["a1", "b2"]} onChange={onChange} />);
    fireEvent.click(screen.getByRole("button", { name: /remove a1/i }));
    expect(onChange).toHaveBeenLastCalledWith(["b2"]);
  });

  it("rejects malformed tags silently (does not commit)", () => {
    const onChange = vi.fn();
    render(<Harness onChange={onChange} />);
    const input = screen.getByRole("textbox") as HTMLInputElement;
    // single char fails the kebab regex (min 2)
    fireEvent.change(input, { target: { value: "a" } });
    fireEvent.keyDown(input, { key: "Enter" });
    expect(onChange).not.toHaveBeenCalled();
    // input retains text so the user can fix it.
    expect(input.value).toBe("a");
  });

  it("caps at 8 tags", () => {
    const onChange = vi.fn();
    const eight = ["a1", "b2", "c3", "d4", "e5", "f6", "g7", "h8"];
    render(<Harness initial={eight} onChange={onChange} />);
    const input = screen.getByRole("textbox") as HTMLInputElement;
    fireEvent.change(input, { target: { value: "i9" } });
    fireEvent.keyDown(input, { key: "Enter" });
    expect(onChange).not.toHaveBeenCalled();
  });
});
