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

export function getClientApiBaseUrl(): string {
  // NEXT_PUBLIC_API_URL is the browser-reachable gateway URL
  // In K8s: set to http://localhost:38800 (NodePort)
  // In Aspire: the gateway is proxied, so same origin works
  return process.env.NEXT_PUBLIC_API_URL || "";
}

