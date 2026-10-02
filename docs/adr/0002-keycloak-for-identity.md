# Keycloak for identity; selling rights stay in our data

Sign-in is handled by Keycloak, run as an Aspire resource. The Next.js frontend signs in via OIDC and the API validates Keycloak-issued JWTs, mapping each token subject to an Account. Keycloak only answers "who is this?" plus the Platform Operator role. Whether an Account owns a Merchant (and, later, which Merchants it is staff of) lives in our own database, not in Keycloak roles or groups, so that opening a Shop or adding staff never requires provisioning in the identity provider.

## Considered Options

- **ASP.NET Core Identity inside the API**: no extra container, but sign-in UI and token issuing would be hand-built and tangled with domain code.
- **Hosted provider (Auth0, Entra ID, Clerk)**: least work, but locks us to a vendor and needs internet access during local development.
