import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { getMarketingDraft } from "@/lib/seller/data";
import { EmailComposerAudience } from "./email-composer-audience";

describe("EmailComposerAudience", () => {
  const draft = getMarketingDraft();

  it("renders step label", () => {
    render(<EmailComposerAudience draft={draft} />);
    expect(screen.getByText(/Step 1 of 3 · Audience/i)).toBeInTheDocument();
  });

  it("renders Recipients heading", () => {
    render(<EmailComposerAudience draft={draft} />);
    expect(screen.getByText("Recipients")).toBeInTheDocument();
  });

  it("renders draft.deliverable", () => {
    render(<EmailComposerAudience draft={draft} />);
    expect(screen.getByText(draft.deliverable)).toBeInTheDocument();
  });

  it("renders all audience labels", () => {
    render(<EmailComposerAudience draft={draft} />);
    for (const audience of draft.audiences) {
      expect(screen.getByText(audience.label)).toBeInTheDocument();
    }
  });

  it("renders all audience sub-labels", () => {
    render(<EmailComposerAudience draft={draft} />);
    for (const audience of draft.audiences) {
      expect(screen.getByText(audience.sub)).toBeInTheDocument();
    }
  });

  it("renders all audience counts", () => {
    render(<EmailComposerAudience draft={draft} />);
    for (const audience of draft.audiences) {
      expect(
        screen.getAllByText(String(audience.count)).length,
      ).toBeGreaterThanOrEqual(1);
    }
  });

  it("renders one row per audience", () => {
    const { container } = render(<EmailComposerAudience draft={draft} />);
    const rows = container.querySelectorAll("[class*='rounded-[10px]']");
    expect(rows.length).toBe(draft.audiences.length);
  });

  it("does not contain 'Mira'", () => {
    const { container } = render(<EmailComposerAudience draft={draft} />);
    expect(container.textContent).not.toContain("Mira");
  });
});
