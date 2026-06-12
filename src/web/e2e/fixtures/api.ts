// Resolves the Catalog API base URL from Aspire's injected env var first,
// then a developer override, then a sensible local default. The Aspire
// service name contains a hyphen, so bracket-access is required.
export const API_URL =
  process.env["services__catalog-api__http__0"] ??
  process.env.API_URL ??
  "http://localhost:5481";

export const productEndpoint = (sku: string) =>
  `${API_URL}/api/products/${encodeURIComponent(sku)}`;
