import AxeBuilder from "@axe-core/playwright";
import { expect, type Page } from "@playwright/test";

export async function expectNoSeriousA11yViolations(page: Page) {
  const { violations } = await new AxeBuilder({ page }).analyze();
  const seriousViolations = violations.filter((violation) =>
    violation.impact === "serious" || violation.impact === "critical",
  );
  const summary = seriousViolations
    .map((violation) => `${violation.id}: ${violation.nodes.map((node) => node.target.join(", ")).join("; ")}`)
    .join("\n");

  expect(seriousViolations, summary).toEqual([]);
}
