// Mints a real seller access token via the Keycloak password grant, so e2e
// specs can attach `Authorization: Bearer <token>` to the Catalog API's
// write endpoints (POST/DELETE /api/promotions, etc.) which are now gated by
// the `seller` role. Public reads stay tokenless.
//
// Credentials and the Keycloak base URL come from env (so the AppHost / CI can
// inject them) with the seeded dev-realm values as the literal fallback. Keep
// these in sync with src/AppHost/Realms/microcommerce-realm.json and
// e2e/auth.setup.ts.

import type { APIRequestContext } from "@playwright/test";

const KEYCLOAK_URL = process.env.KEYCLOAK_URL ?? "http://localhost:8080";
const SELLER_EMAIL = process.env.E2E_SELLER_EMAIL ?? "seller@microcommerce.dev";
const SELLER_PASSWORD = process.env.E2E_SELLER_PASSWORD ?? "Passw0rd!";

const TOKEN_ENDPOINT = `${KEYCLOAK_URL}/realms/microcommerce/protocol/openid-connect/token`;

/**
 * Performs the Keycloak Resource Owner Password Credentials grant for the
 * seeded dev seller and returns the raw `access_token` (a JWT carrying
 * `roles: ["seller"]`). Throws if Keycloak refuses the grant so a missing
 * realm/user surfaces loudly instead of as a downstream 401.
 */
export async function getSellerToken(
  request: APIRequestContext,
): Promise<string> {
  const res = await request.post(TOKEN_ENDPOINT, {
    headers: { "Content-Type": "application/x-www-form-urlencoded" },
    form: {
      grant_type: "password",
      client_id: "microcommerce-web",
      client_secret: "dev-only-web-secret",
      username: SELLER_EMAIL,
      password: SELLER_PASSWORD,
      scope: "openid",
    },
  });

  if (!res.ok()) {
    throw new Error(
      `getSellerToken failed: ${res.status()} ${await res.text()}`,
    );
  }

  const body = (await res.json()) as { access_token?: string };
  if (!body.access_token) {
    throw new Error("getSellerToken: response had no access_token");
  }
  return body.access_token;
}
