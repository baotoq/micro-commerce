import { apiFetch } from "@/lib/api";

// Mirrors MeResponse in the API.
export type Me = {
  id: string;
  email: string | null;
  displayName: string;
  isPlatformOperator: boolean;
};

// Server-side: the caller passes the session's access token. The first call creates the Account.
export function getMe(accessToken: string) {
  return apiFetch<Me>("/me", { headers: { Authorization: `Bearer ${accessToken}` } });
}
