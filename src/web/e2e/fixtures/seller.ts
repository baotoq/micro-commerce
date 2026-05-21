import type { Page } from "@playwright/test";

export const sellerRoutes = {
  dashboard: "/seller",
  listings: "/seller/listings",
  listingsNew: "/seller/listings/new",
  listingsBulk: "/seller/listings/bulk",
  listingsPublished: "/seller/listings/published",
  listingEdit: (sku: string) =>
    `/seller/listings/${encodeURIComponent(sku)}/edit`,
  listingPreview: (sku: string) =>
    `/seller/listings/${encodeURIComponent(sku)}/preview`,
  promos: "/seller/promos",
  orders: "/seller/orders",
  payouts: "/seller/payouts",
  analytics: "/seller/analytics",
  marketing: "/seller/marketing",
  welcome: "/seller/welcome",
  apply: "/seller/apply",
} as const;

// shadcn Select renders as <button role="combobox"> + Radix listbox, so
// Playwright's `selectOption` (which targets native <select>) fails. Open the
// trigger then click the option by visible name.
export const selectShadcnOption = async (
  page: Page,
  labelText: string,
  optionName: string,
) => {
  await page.getByLabel(labelText).click();
  await page.getByRole("option", { name: optionName }).click();
};
