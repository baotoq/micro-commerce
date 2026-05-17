import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import { getMarketingDraft } from "@/lib/seller/data";
import { EmailComposerSchedule } from "./email-composer-schedule";

describe("EmailComposerSchedule", () => {
  const draft = getMarketingDraft();

  it("renders step label", () => {
    render(<EmailComposerSchedule draft={draft} />);
    expect(screen.getByText(/Step 3 of 3 · Schedule/i)).toBeInTheDocument();
  });

  it("renders Send section label", () => {
    render(<EmailComposerSchedule draft={draft} />);
    expect(screen.getByText("Send")).toBeInTheDocument();
  });

  it("renders all schedule option labels", () => {
    render(<EmailComposerSchedule draft={draft} />);
    for (const opt of draft.schedule) {
      expect(screen.getByText(opt.label)).toBeInTheDocument();
    }
  });

  it("renders all schedule option sub-labels", () => {
    render(<EmailComposerSchedule draft={draft} />);
    for (const opt of draft.schedule) {
      expect(screen.getByText(opt.sub)).toBeInTheDocument();
    }
  });

  it("renders Follow-ups section label", () => {
    render(<EmailComposerSchedule draft={draft} />);
    expect(screen.getByText("Follow-ups")).toBeInTheDocument();
  });

  it("renders all followup labels", () => {
    render(<EmailComposerSchedule draft={draft} />);
    for (const f of draft.followups) {
      expect(screen.getByText(f.label)).toBeInTheDocument();
    }
  });

  it("does not contain 'Mira'", () => {
    const { container } = render(<EmailComposerSchedule draft={draft} />);
    expect(container.textContent).not.toContain("Mira");
  });
});
