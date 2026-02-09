"use client";

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { ExternalLink } from "lucide-react";

export default function SecurityPage() {
  // Construct Keycloak account URL from issuer or use env var
  const keycloakAccountUrl =
    process.env.NEXT_PUBLIC_KEYCLOAK_ACCOUNT_URL ||
    process.env.NEXT_PUBLIC_KEYCLOAK_ISSUER?.replace("/realms/", "/realms/") + "/account" ||
    "#";

  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-semibold">Security</h2>

      <Card>
        <CardHeader>
          <CardTitle>Password & Authentication</CardTitle>
          <CardDescription>
            Manage your password and security settings through Keycloak
          </CardDescription>
        </CardHeader>
        <CardContent>
          <p className="mb-4 text-sm text-zinc-600">
            You can change your password, enable two-factor authentication, and manage
            your active sessions through the Keycloak account management console.
          </p>
          <Button asChild>
            <a
              href={keycloakAccountUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="inline-flex items-center gap-2"
            >
              Manage Security Settings
              <ExternalLink className="h-4 w-4" />
            </a>
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}
