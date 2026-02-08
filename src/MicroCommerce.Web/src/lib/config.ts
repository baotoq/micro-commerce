// Server-side config that reads Aspire-injected environment variables
export function getApiBaseUrl(): string {
  // Aspire injects gateway service URL (gateway is the API entry point)
  const aspireUrl =
    process.env.services__gateway__https__0 ||
    process.env.services__gateway__http__0;

  if (aspireUrl) {
    return aspireUrl;
  }

  // Fallback for local development without Aspire
  return process.env.API_URL || "http://localhost:5200";
}

