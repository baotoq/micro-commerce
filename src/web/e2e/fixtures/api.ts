// Resolves the Catalog API base URL from Aspire's injected env var first,
// then a developer override, then a sensible local default. Centralising this
// here keeps the 28 spec files free of duplicated env-resolution logic.
export const API_URL =
  process.env.services__catalog_api__http__0 ??
  process.env.API_URL ??
  "http://localhost:3000";

export const productEndpoint = (sku: string) =>
  `${API_URL}/api/products/${encodeURIComponent(sku)}`;
