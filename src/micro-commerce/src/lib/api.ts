export type ProblemDetails = {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  // FastEndpoints validation failures; `name` is the camelCased request property.
  errors?: { name: string; reason: string; code?: string }[];
};

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly problem?: ProblemDetails,
  ) {
    super(problem?.title ?? `API request failed with status ${status}`);
    this.name = "ApiError";
  }
}

// The browser goes through the /api proxy (src/proxy.ts); the server calls the API directly,
// since relative URLs don't resolve in server-side fetch and API_URL is server-only.
function baseUrl() {
  if (typeof window !== "undefined") return "/api";
  const apiUrl = process.env.API_URL;
  if (!apiUrl) throw new Error("API_URL is not configured");
  return apiUrl;
}

export async function apiFetch<T>(path: string, init?: RequestInit): Promise<T> {
  const headers = new Headers(init?.headers);
  if (init?.body && !headers.has("Content-Type")) headers.set("Content-Type", "application/json");

  const response = await fetch(`${baseUrl()}${path}`, { ...init, headers });

  if (!response.ok) {
    const problem = response.headers.get("content-type")?.includes("json")
      ? ((await response.json()) as ProblemDetails)
      : undefined;
    throw new ApiError(response.status, problem);
  }

  if (response.status === 204) return undefined as T;
  return (await response.json()) as T;
}
