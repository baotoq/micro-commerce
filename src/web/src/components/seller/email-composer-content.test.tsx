import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { getMarketingDraft } from "@/lib/seller/marketing/data";
import { EmailComposerContent } from "./email-composer-content";

describe("EmailComposerContent", () => {
  const draft = getMarketingDraft();

  it("renders step label", () => {
    render(<EmailComposerContent draft={draft} />);
    expect(screen.getByText(/Step 2 of 3 · Content/i)).toBeInTheDocument();
  });

  it("renders Template label", () => {
    render(<EmailComposerContent draft={draft} />);
    expect(screen.getByText("Template")).toBeInTheDocument();
  });

  it("renders all template labels", () => {
    render(<EmailComposerContent draft={draft} />);
    for (const t of draft.templates) {
      expect(screen.getByText(t.label)).toBeInTheDocument();
    }
  });

  it("renders Subject label", () => {
    render(<EmailComposerContent draft={draft} />);
    expect(screen.getByText("Subject")).toBeInTheDocument();
  });

  it("renders draft.subject", () => {
    render(<EmailComposerContent draft={draft} />);
    expect(screen.getByText(draft.subject)).toBeInTheDocument();
  });

  it("renders open-rate forecast text", () => {
    render(<EmailComposerContent draft={draft} />);
    expect(screen.getByText(/Open-rate forecast/i)).toBeInTheDocument();
    expect(screen.getByText("32%")).toBeInTheDocument();
  });

  it("renders subject char count", () => {
    render(<EmailComposerContent draft={draft} />);
    expect(
      screen.getByText(`${draft.subjectCharCount} / ${draft.subjectMaxChars}`),
    ).toBeInTheDocument();
  });

  it("renders Preview text label", () => {
    render(<EmailComposerContent draft={draft} />);
    expect(screen.getByText("Preview text")).toBeInTheDocument();
  });

  it("renders draft.previewText", () => {
    render(<EmailComposerContent draft={draft} />);
    expect(screen.getByText(draft.previewText)).toBeInTheDocument();
  });

  it("renders Featured product label", () => {
    render(<EmailComposerContent draft={draft} />);
    expect(screen.getByText("Featured product")).toBeInTheDocument();
  });

  it("renders draft.productName", () => {
    render(<EmailComposerContent draft={draft} />);
    expect(screen.getByText(draft.productName)).toBeInTheDocument();
  });

  it("renders draft.productInventoryLabel", () => {
    render(<EmailComposerContent draft={draft} />);
    expect(screen.getByText(draft.productInventoryLabel)).toBeInTheDocument();
  });

  it("renders Change button", () => {
    render(<EmailComposerContent draft={draft} />);
    expect(screen.getByRole("button", { name: "Change" })).toBeInTheDocument();
  });

  it("does not contain 'Mira'", () => {
    const { container } = render(<EmailComposerContent draft={draft} />);
    expect(container.textContent).not.toContain("Mira");
  });
});
