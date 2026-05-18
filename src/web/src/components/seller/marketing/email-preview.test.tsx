import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { BRAND } from "@/lib/seller/brand";
import { getMarketingDraft } from "@/lib/seller/marketing/data";
import { EmailPreview } from "./email-preview";

describe("EmailPreview", () => {
  const draft = getMarketingDraft();

  it("renders draft.subject", () => {
    render(<EmailPreview draft={draft} />);
    expect(screen.getAllByText(draft.subject).length).toBeGreaterThanOrEqual(1);
  });

  it("renders draft.greeting", () => {
    render(<EmailPreview draft={draft} />);
    expect(screen.getByText(draft.greeting)).toBeInTheDocument();
  });

  it("renders all body paragraphs", () => {
    render(<EmailPreview draft={draft} />);
    for (const para of draft.body) {
      expect(screen.getByText(para)).toBeInTheDocument();
    }
  });

  it("renders draft.ctaLabel", () => {
    render(<EmailPreview draft={draft} />);
    expect(screen.getByText(draft.ctaLabel)).toBeInTheDocument();
  });

  it("renders draft.signoff", () => {
    render(<EmailPreview draft={draft} />);
    expect(screen.getByText(draft.signoff)).toBeInTheDocument();
  });

  it("renders draft.senderName", () => {
    render(<EmailPreview draft={draft} />);
    expect(screen.getAllByText(draft.senderName).length).toBeGreaterThanOrEqual(
      1,
    );
  });

  it("does NOT contain 'Mira'", () => {
    const { container } = render(<EmailPreview draft={draft} />);
    expect(container.textContent).not.toContain("Mira");
  });

  it("contains BRAND.name", () => {
    const { container } = render(<EmailPreview draft={draft} />);
    expect(container.textContent).toContain(BRAND.name);
  });
});
