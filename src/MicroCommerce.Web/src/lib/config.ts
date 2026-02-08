// Server-side config that reads Aspire-injected environment variables
export function getApiBaseUrl(): string {
  // Aspire injects service URLs in this format
  const aspireUrl =
    process.env.services__apiservice__https__0 ||
    process.env.services__apiservice__http__0;

  if (aspireUrl) {
    return aspireUrl;
  }

  // Fallback for local development without Aspire
  return process.env.API_URL || "http://localhost:5182";
}

